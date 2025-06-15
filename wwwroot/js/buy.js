function filterBySector() {
    const selected = document.getElementById("sectorFilter").value;
    const items = document.querySelectorAll(".sector-item");

    items.forEach(item => {
        const sectorId = item.dataset.sectorId;
        const show = selected === "all" || sectorId === selected;
        item.style.display = show ? "block" : "none";
    });
}

let selectedSeats = [];
let currentSectorId = null;

function getValidCart() {
    const now = Date.now();
    const cart = JSON.parse(localStorage.getItem("ticketCart")) || [];
    const validCart = cart.filter(item => item.expiresAt && item.expiresAt > now);
    localStorage.setItem("ticketCart", JSON.stringify(validCart));
    return validCart;
}

async function openSeatModal(eventSectorId) {
    currentSectorId = eventSectorId;
    selectedSeats = [];

    const cart = getValidCart();

    const userId = localStorage.getItem("userId");
    const cartId = localStorage.getItem("cartId");

    const hasSeatsInCart = cart.some(item =>
        Number(item.eventSectorId) === Number(currentSectorId)
    );

    document.getElementById("legend-in-cart").style.display = hasSeatsInCart ? "inline-flex" : "none";


    const modal = new bootstrap.Modal(document.getElementById('seatModal'));
    const grid = document.getElementById('seatGrid');
    grid.innerHTML = "";

    console.log("userId:", userId);
    console.log("cartId:", cartId);

    // 1. Get seat config & occupied seats
    const configResp = await fetch(`/api/eventsector/config/${eventSectorId}`);
    const config = await configResp.json();

    document.getElementById("seatSectorName").innerText = config.subsectorName;
    document.getElementById("seatPriceBadge").innerText = `${config.price.toFixed(2)} RON`;

    const takenResp = await fetch(`/api/ticket/occupied-seats/${eventSectorId}?userId=${userId}&cartId=${cartId}`)
    const takenSeats = await takenResp.json();
    console.log("Locuri ocupate venite din backend:", takenSeats);


    for (let r = 1; r <= config.rows; r++) {
        const rowDiv = document.createElement('div');
        rowDiv.classList.add('d-flex', 'gap-1', 'align-items-center');

        const rowLabel = document.createElement('span');
        rowLabel.innerText = r.toString().padStart(2, '0');
        rowLabel.classList.add('me-2', 'fw-bold', 'text-end');
        rowLabel.style.width = "2rem";
        rowDiv.appendChild(rowLabel);

        for (let s = 1; s <= config.seatsPerRow; s++) {
            const btn = document.createElement('button');
            btn.classList.add('btn', 'btn-sm', 'seat-button');
            btn.innerText = s;
            btn.dataset.row = r;
            btn.dataset.seat = s;

            const seatStatus = takenSeats.find(ts => ts.row === r && ts.seat === s)?.type;

            if (seatStatus === "in-cart") {
                btn.classList.add("in-cart");
                btn.disabled = true;
            } else if (seatStatus === "occupied") {
                btn.classList.add("occupied");
                btn.disabled = true;
            } else {
                btn.classList.add("available");
                btn.addEventListener("click", () => toggleSeat(btn));
            }

            rowDiv.appendChild(btn);
        }
        grid.appendChild(rowDiv);
    }

    modal.show();
}

function toggleSeat(button) {
    const row = parseInt(button.dataset.row);
    const seat = parseInt(button.dataset.seat);
    const index = selectedSeats.findIndex(x => x.row === row && x.seat === seat);

    if (index > -1) {
        selectedSeats.splice(index, 1);
        button.classList.remove('selected');
        button.classList.add('available');
    } else {
        selectedSeats.push({ row, seat });
        button.classList.remove('available');
        button.classList.add('selected');
    }
}

function confirmSeats() {
    console.log("Locuri selectate:", selectedSeats);
    
    bootstrap.Modal.getInstance(document.getElementById('seatModal')).hide();
}


async function addToCart() {
    if (selectedSeats.length === 0) {
        alert("Nu ai selectat niciun loc.");
        return;
    }

    const userId = localStorage.getItem("userId");
    let cartId = null;

    // Dacă userul NU este logat, folosim cartId
    if (!userId) {
        cartId = localStorage.getItem("cartId");

        if (!cartId) {
            cartId = crypto.randomUUID();
            localStorage.setItem("cartId", cartId);
        }
    }

    const response = await fetch("/api/cart/temp-block", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
            userId: userId,
            cartId: cartId,
            eventSectorId: currentSectorId,
            seats: selectedSeats
        })
    });

    if (!response.ok) {
        alert("Eroare la adăugarea în coș.");
        return;
    }

    const responseData = await response.json(); // Lista de bilete returnate
    const expiresAt = Date.now() + 15 * 60 * 1000; // 15 minute în ms

    const cartItems = responseData.map(item => ({
        ...item,
        expiresAt,
        eventSectorId: currentSectorId
    }));
    
    const existingCart = JSON.parse(localStorage.getItem("ticketCart")) || [];
    const updatedCart = [...existingCart, ...cartItems];

    localStorage.setItem("ticketCart", JSON.stringify(updatedCart));

    
    showToast("Locurile au fost adăugate în coș pentru 15 minute.", "info");
    bootstrap.Modal.getInstance(document.getElementById('seatModal')).hide();
}
