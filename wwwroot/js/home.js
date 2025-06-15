let suggestedEventIds = [];

window.addEventListener("DOMContentLoaded", () => {
    const isLoggedIn = document.body.getAttribute("data-user-logged-in") === "true";

    


    const favoriteLink = document.querySelector('a[href="/Favorites"]');

    if (favoriteLink) {
        favoriteLink.addEventListener("click", function (e) {
            if (!isLoggedIn) {
                e.preventDefault();

                const alertBox = document.createElement('div');
                alertBox.className = 'alert alert-warning text-center fixed-top';
                alertBox.style.zIndex = '9999';
                alertBox.innerHTML = `
                    <strong>Trebuie să fii logat</strong> pentru a accesa pagina de favorite.
                    <button type="button" class="btn-close float-end" onclick="this.parentElement.style.display='none';"></button>
                `;
                document.body.prepend(alertBox);
            }
        });
    }
    (async () => {
        const isLoggedIn = document.body.getAttribute("data-user-logged-in") === "true";

        let excludedIds = [];

        if (isLoggedIn) {
            excludedIds = await loadSuggestedEvents();
        }

        loadAllEventsGroupedBySport(excludedIds);
    })();



});

let allLocations = [];

document.addEventListener("DOMContentLoaded", async function () {
    try {
        const response = await fetch("api/Event/filters");
        const data = await response.json();

        // SPORTURI
        const sportContainer = document.querySelector("#filter-sports");
        if (sportContainer && data.sports) {
            sportContainer.innerHTML = "";
            data.sports.forEach((sport, index) => {
                const id = `f${index + 1}`;
                sportContainer.innerHTML += `
                    <div class="form-check">
                        <input type="checkbox" class="form-check-input" id="${id}" value="${sport}">
                        <label class="form-check-label" for="${id}">${sport}</label>
                    </div>`;
            });
        }

        // ORAȘE
        const citySelect = document.querySelector("#filter-cities");
        if (citySelect && data.cities) {
            citySelect.innerHTML = `<option value="">Toate orașele</option>`;
            data.cities.forEach(city => {
                citySelect.innerHTML += `<option value="${city}">${city}</option>`;
            });
            citySelect.addEventListener("change", () => {
                updateLocationOptions();
                applyFilters();
            });
        }

        // LOCAȚII
        allLocations = data.locations;
        updateLocationOptions(); // populăm locațiile la început
    } catch (err) {
        console.error("Eroare la încărcarea filtrelor:", err);
    }
});

function updateLocationOptions() {
    const city = document.querySelector("#filter-cities").value;
    const locationSelect = document.querySelector("#filter-locations");

    locationSelect.innerHTML = `<option value="">Toate locațiile</option>`;

    const filtered = city
        ? allLocations.filter(loc => loc.city === city)
        : allLocations;

    filtered.forEach(loc => {
        locationSelect.innerHTML += `<option value="${loc.locationName}">${loc.locationName}</option>`;
    });
}



function applyFilters() {
    const checkedSports = Array.from(document.querySelectorAll("#filter-sports input:checked"))
        .map(cb => cb.nextElementSibling.innerText);

    const selectedCity = document.querySelector("#filter-cities")?.value || "";
    const selectedLocation = document.querySelector("#filter-locations")?.value || "";
    const startDate = document.getElementById("filter-start-date")?.value;
    const endDate = document.getElementById("filter-end-date")?.value;

    const params = new URLSearchParams();
    if (checkedSports.length > 0) {
        params.append("sportsJson", JSON.stringify(checkedSports));
    }
    if (selectedCity) params.append("city", selectedCity);
    if (selectedLocation) params.append("location", selectedLocation);
    if (startDate) params.append("startDate", startDate);
    if (endDate) params.append("endDate", endDate);

    fetch(`/api/Event/filter?${params.toString()}`)
        .then(res => res.json())
        .then(events => {
            const container = document.querySelector("#events-container");
            if (!container) return;

            container.innerHTML = "";

            let filteredEvents = events;
            if (!areFiltersActive()) {
                filteredEvents = events.filter(ev => !suggestedEventIds.includes(ev.eventId));
            }
            

            if (!filteredEvents.length) {
                container.innerHTML = "<p class='text-muted'>Nu există evenimente care să corespundă filtrelor.</p>";
                return;
            }

            const grouped = {};
            filteredEvents.forEach(ev => {
                if (!grouped[ev.sportType]) grouped[ev.sportType] = [];
                grouped[ev.sportType].push(ev);
            });

            for (const sport in grouped) {
                container.innerHTML += `<h3 class="fw-bold mt-5">${sport}</h3><div class="row g-4 mb-4" id="sport-${sport}"></div>`;
                const sportContainer = document.querySelector(`#sport-${sport}`);

                grouped[sport].forEach(e => {
                    sportContainer.innerHTML += `
                        <div class="col-md-6 col-lg-4">
                            <div class="card event-card h-100">
                                <img src="${e.imageUrl || "/images/default-event.jpg"}" class="card-img-top" alt="Imagine eveniment">
                                <div class="card-body d-flex flex-column">
                                    <h5 class="card-title">${e.participant1Name} vs ${e.participant2Name}</h5>
                                    <p class="text-muted mb-1">${new Date(e.eventDate).toLocaleDateString("ro-RO")}</p>
                                    <p class="text-secondary mb-3">${e.locationName}</p>
                                    <a href="/Event/Details/${e.eventId}" class="btn btn-warning mt-auto w-100">Detalii</a>
                                </div>
                            </div>
                        </div>`;
                });
            }
        })
        .catch(err => console.error("Eroare la filtrarea evenimentelor:", err));
}


async function loadSuggestedEvents() {
    const token = localStorage.getItem("jwt");
    if (!token) {
        console.warn("Token JWT lipsă din localStorage.");
        return;
    }
    console.log(" Token găsit:", token);
    console.log(" Trimit request către /api/Favorite/suggestions");
    try {
        const response = await fetch("/api/Favorite/suggestions", {
            headers: {
                Authorization: `Bearer ${token}`
            }
        });
        
        const events = await response.json();

        if (!events || events.length === 0) {
            updateSuggestedEventsVisibility();
            return [];
        }

        const wrapper = document.getElementById("suggested-events-wrapper");
        if (!wrapper) return [];

        wrapper.innerHTML = `
            <h2 class="fw-bold mb-4">Evenimente ale echipelor tale preferate</h2>
            <div class="row g-4 mb-5" id="suggested-events-list"></div>
        `;

        const list = document.getElementById("suggested-events-list");

        events.forEach(e => {
            list.innerHTML += `
                <div class="col-md-6 col-lg-4">
                    <div class="event-info-card home-event-card h-100 d-flex flex-column text-center">
                        <img src="${e.imageUrl || '/images/default-event.jpg'}"
                            alt="${e.participant1Name} vs ${e.participant2Name}"
                            style="width: 100%; aspect-ratio: 3 / 2; object-fit: cover; border-radius: 12px;" />

                        <h5 class="fw-bold mt-3">${e.participant1Name} vs ${e.participant2Name}</h5>

                        <p class="small text-muted mb-1">
                            <i class="bi bi-calendar-event me-1"></i>
                            ${new Date(e.eventDate).toLocaleDateString("ro-RO")}
                        </p>

                        <p class="small text-muted">
                            <i class="bi bi-geo-alt me-1"></i>
                            ${e.locationName}
                        </p>

        

                        <a class="btn btn-warning w-100 mt-3 fw-bold" href="/Event/Details/${e.eventId}">
                            Detalii
                        </a>
                    </div>
                </div>`;
        });

        updateSuggestedEventsVisibility();
        suggestedEventIds = events.map(e => e.eventId);
        return suggestedEventIds;
    } catch (err) {
        console.error("Eroare la încărcarea sugestiilor:", err);
    }
}


async function loadAllEventsGroupedBySport(excludedIds = []) {
    try {
        const response = await fetch("/api/Event");
        const events = await response.json();

        const container = document.querySelector("#events-container");
        if (!container) return;

        const title = document.querySelector("#events-title");

        const now = new Date();

        const filteredEvents = events
            .filter(ev => !excludedIds.includes(ev.eventId))
            .filter(ev => new Date(ev.eventDate) > now);

        if (!filteredEvents.length) {
            container.innerHTML = "<p class='text-muted'>Nu există evenimente disponibile momentan.</p>";
            if (title) title.innerText = "Evenimente";
            return;
        }

        container.innerHTML = "";

        if (excludedIds.length && title) {
            title.innerText = "Alte evenimente";
        } else if (title) {
            title.innerText = "Evenimente";
        }

        const grouped = {};
        filteredEvents.forEach(ev => {
            if (!grouped[ev.sportType]) grouped[ev.sportType] = [];
            grouped[ev.sportType].push(ev);
        });

        for (const sport in grouped) {
            container.innerHTML += `<h3 class="fw-bold mt-5">${sport}</h3><div class="row g-4 mb-4" id="sport-${sport}"></div>`;
            const sportContainer = document.getElementById(`sport-${sport}`);

            grouped[sport].forEach(e => {
                sportContainer.innerHTML += `
                    <div class="col-md-6 col-lg-4">
                        <div class="event-info-card home-event-card h-100 d-flex flex-column text-center">
                        <img src="${e.imageUrl || '/images/default-event.jpg'}"
                            alt="${e.participant1Name} vs ${e.participant2Name}"
                                style="width: 100%; aspect-ratio: 3 / 2; object-fit: cover; border-radius: 12px;" />

                        <h5 class="fw-bold mt-3">${e.participant1Name} vs ${e.participant2Name}</h5>

                        <p class="small text-muted mb-1">
                            <i class="bi bi-calendar-event me-1"></i>
                            ${new Date(e.eventDate).toLocaleDateString("ro-RO")}
                        </p>

                        <p class="small text-muted">
                            <i class="bi bi-geo-alt me-1"></i>
                            ${e.locationName}
                        </p>


                        <a class="btn btn-warning w-100 mt-3 fw-bold" href="/Event/Details/${e.eventId}">
                            Detalii
                        </a>
                    </div>
                </div>`;
            });
        }
    } catch (err) {
        console.error("Eroare la încărcarea evenimentelor:", err);
    }
}

function areFiltersActive() {
    return (
        document.querySelectorAll("#filter-sports input:checked").length > 0 ||
        document.getElementById("filter-cities").value !== "" ||
        document.getElementById("filter-locations").value !== "" ||
        document.getElementById("filter-start-date").value !== "" ||
        document.getElementById("filter-end-date").value !== ""
    );
}



function updateSuggestedEventsVisibility() {
    const hasFilters =
        document.querySelectorAll("#filter-sports input[type='checkbox']:checked").length > 0 ||
        document.getElementById("filter-cities").value !== "" ||
        document.getElementById("filter-locations").value !== "" ||
        document.getElementById("filter-start-date").value !== "" ||
        document.getElementById("filter-end-date").value !== "";

    const wrapper = document.getElementById("suggested-events-wrapper");
    if (!wrapper) return;

    wrapper.style.display = hasFilters ? "none" : "block";
}

document.addEventListener("change", (e) => {
    if (
        e.target.closest("#filter-sports") ||
        e.target.matches("#filter-cities") ||
        e.target.matches("#filter-locations") ||
        e.target.matches("#filter-start-date") ||
        e.target.matches("#filter-end-date")
    ) {
        applyFilters();
        updateSuggestedEventsVisibility();
    }
});






