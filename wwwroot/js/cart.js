document.addEventListener("DOMContentLoaded", async () => {
    const userId = localStorage.getItem("userId");
    const cartId = localStorage.getItem("cartId");

    const cart = JSON.parse(localStorage.getItem("ticketCart")) || [];
    const myTickets = cart.filter(b =>
        (userId && b.userId === userId) ||
        (!userId && b.cartId === cartId)
    );

    console.log("Biletele din backend:", cart);

    const container = document.getElementById("cartContainer");

    if (myTickets.length === 0) {
        container.innerHTML = "<p class='text-muted text-center'>Coșul tău este gol.</p>";
    } else {
        myTickets.forEach((b) => {
            const col = document.createElement("div");
            col.className = "col-md-6";

            const uniqueId = `${b.eventSectorId}-${b.row}-${b.seat}`;
            const expiresAt = b.expiresAt;

            col.innerHTML = `
                <div class="card shadow-sm p-3 h-100 ticket-card" data-expires-at="${expiresAt}" data-unique-id="${uniqueId}">
                    <h5 class="fw-bold">${b.eventName}</h5>
                    <p><strong>Locație:</strong> ${b.locationName}</p>
                    <p><strong>Dată:</strong> ${new Date(b.eventDate).toLocaleDateString()}</p>
                    <p><strong>Sector:</strong> ${b.sectorName} / ${b.subsectorName}</p>
                    <p><strong>Loc:</strong> Rând ${b.row}, Loc ${b.seat}</p>
                    <p><strong>Preț:</strong> ${b.price.toFixed(2)} RON</p>
                    <p class="text-danger fw-bold" id="countdown-${uniqueId}">Calculând timpul...</p>
                    <button class="btn btn-danger btn-sm mt-2 remove-btn" data-id="${uniqueId}">Șterge</button>
                </div>
            `;


            container.appendChild(col);
            col.querySelector(".remove-btn").addEventListener("click", async () => {
                try {
                    // 1. Trimitem request către backend mai întâi
                    const res = await fetch("api/cart/remove-temp-reservation", {
                        method: "POST",
                        headers: {
                            "Content-Type": "application/json"
                        },
                        body: JSON.stringify({
                            userId: b.userId,
                            cartId: b.cartId,
                            eventSectorId: b.eventSectorId,
                            seats: [{ row: b.row, seat: b.seat }]
                        })
                    });

                    if (!res.ok) {
                        alert("Eroare la ștergerea biletului din backend.");
                        return;
                    }

                    // 2. Scoatem din localStorage doar dacă ștergerea a reușit
                    const newCart = cart.filter(x =>
                        !(x.eventSectorId === b.eventSectorId && x.row === b.row && x.seat === b.seat)
                    );
                    localStorage.setItem("ticketCart", JSON.stringify(newCart));

                    // 3. Refacem interfața
                    location.reload();

                } catch (error) {
                    console.error("Eroare la comunicarea cu serverul:", error);
                    alert("A apărut o eroare. Încearcă din nou.");
                }
            });

        });
        startExpireCountdowns(); 

    }

    const form = document.getElementById("checkoutForm");

    form.addEventListener("submit", async (e) => {
        e.preventDefault();

        const formData = new FormData(form);

        const firstName = formData.get("firstName")?.toString().trim() ?? "";
        const lastName = formData.get("lastName")?.toString().trim() ?? "";
        const email = formData.get("email")?.toString().trim() ?? "";
        const ageRaw = formData.get("age")?.toString();
        const age = ageRaw !== "" ? parseInt(ageRaw) : null;

        // 🧪 Debug info
        console.log("Valori colectate (FormData):", {
            firstName,
            lastName,
            email,
            age
        });

        //  Validare
        if (!firstName || !lastName || !email || isNaN(age)) {
            form.reportValidity();
            alert("Completează toate câmpurile!");
            return;
        }

        //  Preluare coș
        const userId = localStorage.getItem("userId");
        const cartId = localStorage.getItem("cartId");
        const cart = JSON.parse(localStorage.getItem("ticketCart")) || [];

        const myTickets = cart.filter(b =>
            (userId && b.userId === userId) ||
            (!userId && b.cartId === cartId)
        );

        if (myTickets.length === 0) {
            alert("Coșul este gol!");
            return;
        }

        

        const ticketDTOs = myTickets.map(b => ({
            eventSectorId: b.eventSectorId,
            firstName,
            lastName,
            email,
            age,
            row: b.row,
            seat: b.seat
        }));

        const response = await fetch("/api/ticket/multiple", {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                "X-CartId": cartId
            },
            body: JSON.stringify(ticketDTOs)
        });

        if (!response.ok) {
            showToast("A apărut o eroare la salvarea biletelor!", "error");
            return;
        }

        showToast("Biletele au fost rezervate! Le vei primi pe email în câteva secunde.", "success");

        localStorage.removeItem("ticketCart");
        if (!userId) localStorage.removeItem("cartId");



        setTimeout(() => {
            localStorage.removeItem("ticketCart");
            if (!userId) localStorage.removeItem("cartId");
            location.reload();
        }, 3000);
    });

});

function startExpireCountdowns() {
    setInterval(() => {
        const now = Date.now();
        const cards = document.querySelectorAll(".ticket-card");

        cards.forEach(card => {
            const expiresAt = parseInt(card.dataset.expiresAt);
            const uniqueId = card.dataset.uniqueId;
            const countdownElem = document.getElementById(`countdown-${uniqueId}`);

            if (!countdownElem) return;

            const remaining = expiresAt - now;

            if (remaining <= 0) {
                countdownElem.textContent = " Biletul a expirat și va fi șters.";
                countdownElem.classList.add("text-muted");
                
            } else {
                const mins = Math.floor(remaining / 60000);
                const secs = Math.floor((remaining % 60000) / 1000);
                countdownElem.textContent = `⏳ Dacă nu se confirmă, biletul va fi șters în ${mins}m ${secs}s`;
            }
        });
    }, 1000);
}

