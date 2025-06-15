async function addToFavorites(participantId) {
    const token = getCookie('jwt');
    console.log("JWT trimis:", token);
    console.log("ParticipantId trimis:", participantId);

    const response = await fetch('/api/Favorite', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'Authorization': 'Bearer ' + (token ?? '')
        },
        body: JSON.stringify({ participantId: participantId }) // FĂRĂ userId
    });

    if (response.ok) {
        location.reload();
    } else {
        alert("Eroare la adăugare în favorite.");
    }
}

async function deleteFavorite(favoriteId) {
    const token = getCookie('jwt');
    console.log("JWT pentru DELETE:", token);
    console.log("FavoriteId trimis pentru ștergere:", favoriteId);

    const response = await fetch(`/api/Favorite/${favoriteId}`, {
        method: 'DELETE',
        headers: {
            'Authorization': 'Bearer ' + (token ?? '')
        }
    });

    if (response.ok) {
        location.reload();
    } else {
        const error = await response.text();
        console.error("Eroare la ștergere:", error);
        alert("Eroare la ștergere din favorite.");
    }
}




function getCookie(name) {
    const value = `; ${document.cookie}`;
    const parts = value.split(`; ${name}=`);
    if (parts.length === 2) return parts.pop().split(';').shift();
}

async function showParticipantEvents(participantId, participantName) {
    try {
        const response = await fetch(`/api/Participant/by-participant/${participantId}`);
        const events = await response.json();

        const title = document.getElementById("participantEventsModalLabel");
        const container = document.getElementById("participantEventsContainer");

        title.innerText = `Evenimentele echipei ${participantName}`;
        container.innerHTML = "";

        if (!events.length) {
            container.innerHTML = `<p class="text-muted">Această echipă nu are evenimente viitoare.</p>`;
        } else {
            events.forEach(e => {
                container.innerHTML += `
                    <div class="col-md-6 col-lg-4">
                        <div class="card event-card h-100">
                            <img src="${e.imageUrl || "/images/default-event.jpg"}" class="card-img-top" style="height:180px; object-fit:cover;" alt="Imagine eveniment">
                            <div class="card-body d-flex flex-column">
                                <h5 class="card-title">${e.participant1Name} vs ${e.participant2Name}</h5>
                                <p class="text-muted mb-1">${new Date(e.eventDate).toLocaleDateString("ro-RO")}</p>
                                <p class="text-secondary mb-3">${e.locationName}</p>
                                <a href="/Event/Details/${e.eventId}" class="btn btn-outline-primary mt-auto w-100">Detalii</a>
                            </div>
                        </div>
                    </div>
                `;
            });
        }

        const modal = new bootstrap.Modal(document.getElementById("participantEventsModal"));
        modal.show();

    } catch (err) {
        console.error("Eroare la încărcarea evenimentelor participantului:", err);
    }
}

