function toggleEventRow(button) {
    console.log("toggleEventRow triggered");
    const row = button.closest("tr");
    const next = row.nextElementSibling;

    if (!next) {
        console.warn("⚠️ Rândul de sectoare nu există!");
        return;
    }

    next.style.display = next.style.display === "none" ? "table-row" : "none";
}

async function openSeatModalAdmin(eventSectorId) {
    console.log("→ openSeatModalAdmin", { eventSectorId });

    const modalEl = document.getElementById('seatModal');
    const grid = document.getElementById('seatGrid');

    if (!modalEl || !grid) {
        console.error("Modalul sau grila NU există în DOM!");
        return;
    }

    console.log("✔️ Modal și grilă găsite în DOM");
    grid.innerHTML = "";

    try {
        const configResp = await fetch(`/api/eventsector/config/${eventSectorId}`);
        const config = await configResp.json();
        console.log("✔️ Config primit:", config);

        const takenResp = await fetch(`/api/ticket/admin/occupied-seats/${eventSectorId}`);
        const takenSeats = await takenResp.json();
        console.log("✔ Locuri ocupate primite:", takenSeats);

        document.getElementById("seatSectorName").innerText = config.subsectorName;
        document.getElementById("seatPriceBadge").innerText = `${config.price.toFixed(2)} RON`;

        for (let r = 1; r <= config.rows; r++) {
            const rowDiv = document.createElement("div");
            rowDiv.classList.add("seat-row");

            const rowLabel = document.createElement("div");
            rowLabel.classList.add("seat-row-label");
            rowLabel.innerText = r.toString().padStart(2, "0");
            rowDiv.appendChild(rowLabel);

            const seatContainer = document.createElement("div");
            seatContainer.classList.add("seat-buttons");

            for (let s = 1; s <= config.seatsPerRow; s++) {
                const btn = document.createElement("button");
                btn.classList.add("seat-button");
                btn.innerText = s;
                btn.disabled = true;

                const seatInfo = takenSeats.find(ts => ts.row === r && ts.seat === s);

                if (seatInfo) {
                    const role = seatInfo.role;

                    if (role === "administrator") {
                        btn.classList.add("unavailable"); 
                    } else {
                        btn.classList.add("occupied"); 
                    }

                    // Salvează datele în dataset pentru click ulterior
                    btn.dataset.row = r;
                    btn.dataset.seat = s;
                    btn.dataset.email = seatInfo.email;
                    btn.dataset.name = `${seatInfo.firstName} ${seatInfo.lastName}`;
                    btn.dataset.role = role;
                    btn.dataset.ticketId = seatInfo.ticketId;
                    btn.disabled = false;

                    btn.addEventListener("click", () => handleSeatClick(btn));
                } else {
                    // Este liber
                    btn.classList.add("available");
                    btn.dataset.row = r;
                    btn.dataset.seat = s;
                    btn.disabled = false;

                    btn.addEventListener("click", () => handleSeatClick(btn , eventSectorId));
                }

                seatContainer.appendChild(btn);
            }

            rowDiv.appendChild(seatContainer);
            grid.appendChild(rowDiv);
        }

        const modal = new bootstrap.Modal(modalEl);
        modal.show();

    } catch (error) {
        console.error(" Eroare la încărcarea locurilor:", error);
        alert("Nu s-au putut încărca locurile.");
    }
}

async function handleSeatClick(btn, eventSectorId) {
    const info = document.getElementById("seatSelectionInfo");
    const rowSpan = document.getElementById("selectedRow");
    const seatSpan = document.getElementById("selectedSeat");
    const userInfo = document.getElementById("selectedUserInfo");
    const actionBtn = document.getElementById("toggleAvailabilityBtn");

    const row = btn.dataset.row;
    const seat = btn.dataset.seat;
    const role = btn.dataset.role;
    const email = btn.dataset.email;
    const name = btn.dataset.name;

    rowSpan.innerText = row;
    seatSpan.innerText = seat;
    info.classList.remove("d-none");

    if (role === "client") {
        userInfo.innerText = `Cumpărat de: ${name} (${email})`;
        actionBtn.classList.add("d-none");
    } else if (role === "administrator") {
        userInfo.innerText = `Indisponibil (setat de: ${name} (${email}))`;
        actionBtn.innerText = " Marchează ca disponibil";
        actionBtn.classList.remove("d-none");

        actionBtn.onclick = async () => {
            const ticketId = btn.dataset.ticketId;
            if (!ticketId) {
                showToast("Biletul nu a putut fi găsit.", "error");
                return;
            }

            const response = await fetch(`/api/ticket/${ticketId}`, { method: "DELETE" });

            if (response.ok) {
                showToast("Locul a fost eliberat!", "success");

                btn.classList.remove("unavailable");
                btn.classList.add("available");
                btn.dataset.role = "";
                btn.dataset.email = "";
                btn.dataset.name = "";
                btn.dataset.ticketId = "";

                handleSeatClick(btn, eventSectorId);
            } else {
                showToast("Eroare la eliberarea locului!", "error");
            }
        };
    } else {
        userInfo.innerText = `Loc liber`;
        actionBtn.innerText = " Marchează ca indisponibil";
        actionBtn.classList.remove("d-none");

        actionBtn.onclick = async () => {
            const user = await getUserProfil();
            if (!user) return;

            const payload = {
                eventSectorId: eventSectorId,
                row: parseInt(row),
                seat: parseInt(seat),
                email: user.email,
                firstName: user.firstName,
                lastName: user.lastName,
                age: user.age || null
            };

            const response = await fetch("/api/ticket", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(payload)
            });

            if (response.ok) {
                const ticket = await response.json();

                showToast("Locul a fost marcat ca indisponibil!", "success");

                btn.classList.remove("available");
                btn.classList.add("unavailable");
                btn.dataset.role = "administrator";
                btn.dataset.email = user.email;
                btn.dataset.name = `${user.firstName} ${user.lastName}`;
                btn.dataset.ticketId = ticket.ticketId;

                handleSeatClick(btn, eventSectorId);
            } else {
                showToast("Eroare la marcarea locului!", "error");
            }
        };
    }
}


async function getUserProfil() {
    const userId = localStorage.getItem("userId");
    if (!userId) {
        alert("Nu ești logat.");
        return null;
    }

    const response = await fetch(`/api/user/${userId}`);
    if (!response.ok) {
        alert("Nu am putut încărca datele contului.");
        return null;
    }

    return await response.json();
}




function openEditLocationModal(id, name, type, city, capacity) {
    document.getElementById("editLocationId").value = id;
    document.getElementById("editLocationName").value = name;
    document.getElementById("editLocationType").value = type;
    document.getElementById("editLocationCity").value = city;
    document.getElementById("editLocationCapacity").value = capacity;
    document.getElementById("locationMessage").classList.add("d-none");

    new bootstrap.Modal(document.getElementById("editLocationModal")).show();
}

async function updateLocation() {
    const id = document.getElementById("editLocationId").value;
    const dto = {
        locationName: document.getElementById("editLocationName").value,
        locationType: document.getElementById("editLocationType").value,
        city: document.getElementById("editLocationCity").value,
        capacity: parseInt(document.getElementById("editLocationCapacity").value)
    };

    const response = await fetch(`/api/location/${id}`, {
        method: "PUT",
        headers: { "Content-Type": "application/json; charset=UTF-8"},
        body: JSON.stringify(dto)
    });

    if (response.status === 204) {
        showToast("Locația a fost actualizată cu succes!", "success");
        setTimeout(() => location.reload(), 1000);
    } else {
        const text = await response.text();
        showToast(text || "Eroare la actualizare.", "error");
    }
}

async function deleteLocation(id) {
    if (!confirm("Ești sigur că vrei să ștergi această locație?")) return;

    const response = await fetch(`/api/location/${id}`, {
        method: "DELETE"
    });

    if (response.status === 204) {
        showToast("Locația a fost ștearsă.", "success");
        setTimeout(() => location.reload(), 1000);
    } else {
        let errorMessage = "Eroare la ștergere.";
        try {
            const data = await response.json();
            if (data?.message) errorMessage = data.message;
        } catch {
            // dacă nu e JSON, rămâne mesajul implicit
        }

        showToast(errorMessage, "error");
    }
}


document.getElementById("createParticipantForm").addEventListener("submit", async (e) => {
    e.preventDefault();
    const form = e.target;
    const formData = new FormData(form);

    const id = formData.get("participantId");
    const method = id ? "PUT" : "POST";
    const url = id ? `/api/participant/${id}` : `/api/participant`;

    const response = await fetch(url, {
        method: method,
        body: formData
    });

    if (response.ok || response.status === 204) {
        showToast(id ? "Participant actualizat!" : "Participant adăugat!", "success");
        setTimeout(() => location.reload(), 800);
    } else {
        const text = await response.text();
        showToast(text || "Eroare la salvare.", "error");
    }
});


async function deleteParticipant(id) {
    if (!confirm("Ești sigur că vrei să ștergi acest participant?")) return;

    const response = await fetch(`/api/participant/${id}`, { method: "DELETE" });

    if (response.status === 204) {
        showToast("Participant șters!", "success");
        setTimeout(() => location.reload(), 800);
    } else {
        let errorMessage = "Eroare la ștergere.";
        try {
            const data = await response.json();
            if (data?.message) errorMessage = data.message;
        } catch {
            // dacă nu e JSON, rămâne mesajul default
        }

        showToast(errorMessage, "error");
    }
}

async function editParticipant(id) {
    const response = await fetch(`/api/participant/${id}`);
    if (!response.ok) return alert("Eroare la încărcare participant.");

    const data = await response.json();

    // Populează câmpurile din formular
    document.getElementById("participantId").value = data.participantId;
    document.querySelector("[name='name']").value = data.name;
    document.querySelector("[name='sportType']").value = data.sportType;

    // Setează imaginea existentă în preview
    const imgPreview = document.getElementById("imagePreview");
    imgPreview.src = data.imageUrl;
    imgPreview.classList.remove("d-none");

    // Deschide modalul
    new bootstrap.Modal(document.getElementById("createParticipantModal")).show();
    
}

function previewParticipantImage(e) {
    const file = e.target.files[0];
    const preview = document.getElementById("imagePreview");

    if (file) {
        const reader = new FileReader();
        reader.onload = () => {
            preview.src = reader.result;
            preview.classList.remove("d-none");
        };
        reader.readAsDataURL(file);
    } else {
        preview.src = "";
        preview.classList.add("d-none");
    }
}

function toggleSectoare(id) {
    const row = document.getElementById(`sectoare-${id}`);
    if (!row) return;
    row.style.display = row.style.display === "none" ? "table-row" : "none";
}

function openCreateParticipantModal() {
    const form = document.getElementById("createParticipantForm");
    form.reset(); 

    document.getElementById("participantId").value = "";        
    document.getElementById("sportType").disabled = false;      

    document.getElementById("imagePreview").src = "";
    document.getElementById("imagePreview").classList.add("d-none");
}

function filterParticipantsBySport() {
    const selected = document.getElementById("sportFilter").value.toLowerCase();
    const rows = document.querySelectorAll("tbody tr[data-sport]");

    rows.forEach(row => {
        const sport = row.getAttribute("data-sport").toLowerCase();
        row.style.display = (selected === "all" || sport === selected) ? "" : "none";
    });
}







