document.addEventListener("DOMContentLoaded", async () => {
    const userId = localStorage.getItem("userId");
    if (!userId) return;

    const userRes = await fetch(`/api/user/${userId}`);
    if (!userRes.ok) return;
    const user = await userRes.json();
    const email = user.email;

    const eventsRes = await fetch(`/api/ticket/by-email/${email}`);
    if (!eventsRes.ok) return;
    const tickets = await eventsRes.json();

    const upcomingContainer = document.getElementById("upcomingEvents");
    const pastContainer = document.getElementById("pastEvents");

    if (!tickets || tickets.length === 0) {
        upcomingContainer.innerHTML = "<p class='text-muted text-center'>Nu ai bilete achiziționate.</p>";
        return;
    }

    // Grupare bilete după eveniment
    const grouped = {}; // key: eventName||eventDate||locationName||imageUrl → listă bilete
    tickets.forEach(ticket => {
        const key = `${ticket.eventId}||${ticket.eventName}||${ticket.eventDate}||${ticket.locationName}||${ticket.imageUrl}`;

        if (!grouped[key]) grouped[key] = [];
        grouped[key].push(ticket);
    });

    const now = new Date();

    const upcomingEvents = [];
    const pastEvents = [];

    for (const key in grouped) {
        const [eventId, eventName, eventDateRaw, locationName, imageUrl] = key.split("||");
        const bilete = grouped[key];
        const eventDate = new Date(eventDateRaw);

        const card = document.createElement("div");
        card.className = "col-md-6 col-lg-4";

        card.innerHTML = `
            <div class="card shadow-sm h-100">
                <img src="${imageUrl}" class="card-img-top" style="height: 200px; object-fit: cover;" alt="${eventName}">
                <div class="card-body d-flex flex-column">
                    <h5 class="card-title fw-bold">${eventName}</h5>
                    <p class="card-text"><strong>Dată:</strong> ${eventDate.toLocaleDateString()}</p>
                    <p class="card-text"><strong>Locație:</strong> ${locationName}</p>
                    <hr />
                    <p class="fw-bold">Locuri cumpărate (${bilete.length}):</p>
                        <div class="badge-container">
    ${bilete.map(b => `
        <span class="badge bg-primary text-white">
            ${b.sectorName} / ${b.subsectorName} • R${b.row}, L${b.seat} • ${b.price.toFixed(2)} RON
        </span>
    `).join("")}
</div>
${eventDate >= now ? `
    <div class="d-flex flex-column gap-2 mt-3">
        <a href="/Event/Details/${eventId}" class="btn btn-outline-primary w-100">Detalii</a>
        <button class="btn btn-outline-danger w-100" onclick='openCancelModal(${JSON.stringify(bilete)})'>Renunță la bilete</button>
        <small class="text-muted text-center">Poți selecta ce bilete nu mai dorești</small>
    </div>
` : ""}


                </div>
            </div>
        `;

        if (eventDate >= now) {
            upcomingEvents.push(card);
        } else {
            pastEvents.push(card);
        }
    }

    // Afișare evenimente viitoare
    if (upcomingEvents.length === 0) {
        upcomingContainer.innerHTML = "<p class='text-muted'>Nu ai bilete la evenimente viitoare.</p>";
    } else {
        // Dacă sunt mai mult de 3, afișează în grilă
        upcomingEvents.forEach(card => upcomingContainer.appendChild(card));
    }

    // Afișare evenimente trecute
    if (pastEvents.length === 0) {
        pastContainer.innerHTML = "<p class='text-muted'>Nu ai bilete la evenimente trecute.</p>";
    } else {
        pastEvents.forEach(card => pastContainer.appendChild(card));
    }
});

let cancelableTickets = [];

function openCancelModal(tickets) {
    cancelableTickets = tickets;
    const form = document.getElementById("cancelTicketsForm");
    form.innerHTML = "";

    tickets.forEach(t => {
        const label = document.createElement("label");
        label.className = "form-check-label d-flex align-items-center gap-2";

        label.innerHTML = `
            <input type="checkbox" class="form-check-input" value="${t.ticketId}">
            ${t.sectorName} / ${t.subsectorName} • R${t.row}, L${t.seat} – ${t.price.toFixed(2)} RON
        `;

        form.appendChild(label);
    });

    const modal = new bootstrap.Modal(document.getElementById("cancelTicketsModal"));
    modal.show();
}

async function submitCancelTickets() {
    const checked = document.querySelectorAll("#cancelTicketsForm input:checked");
    const ids = Array.from(checked).map(i => i.value);

    if (!ids.length) {
        alert("Selectează cel puțin un bilet!");
        return;
    }

    for (const id of ids) {
        await fetch(`/api/Ticket/${id}`, { method: "DELETE" });
    }

    const modalEl = document.getElementById("cancelTicketsModal");
    const modalInstance = bootstrap.Modal.getInstance(modalEl);
    modalInstance.hide();

    modalEl.addEventListener('hidden.bs.modal', () => {
        showToast("Biletele selectate au fost anulate!", "success");

        setTimeout(() => {
            location.reload();
        }, 1500); // aștepți un pic să vadă toast-ul
    }, { once: true });
}


