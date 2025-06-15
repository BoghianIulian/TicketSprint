function openCreateEventModal() {
    //  Resetare câmpuri
    document.getElementById("event-name").value = "";
    document.getElementById("event-sport").innerHTML = "<option disabled selected>Alege un sport</option>";
    document.getElementById("participant-1").innerHTML = "";
    document.getElementById("participant-2").innerHTML = "";
    document.getElementById("event-date").value = "";
    document.getElementById("event-image").value = "";
    document.getElementById("event-description").value = "";
    document.getElementById("event-location").innerHTML = "";
    document.getElementById("subsector-list").innerHTML = "";
    document.getElementById("eventImagePreview").src = "";
    document.getElementById("eventImagePreview").classList.add("d-none");

    //  Deblochează câmpurile (în caz că au fost dezactivate în edit)
    document.getElementById("event-name").disabled = false;
    document.getElementById("event-sport").disabled = false;
    document.getElementById("participant-1").disabled = false;
    document.getElementById("participant-2").disabled = false;
    document.getElementById("event-date").disabled = false;
    document.getElementById("event-image").disabled = false;
    document.getElementById("event-description").disabled = false;
    document.getElementById("event-location").disabled = false;

    // Încarcă dinamic datele
    loadSports();
    loadLocations();

    const modal = new bootstrap.Modal(document.getElementById("createEventModal"));
    modal.show();
}


async function loadSports() {
    const res = await fetch("/api/participant/sports"); 
    const sports = await res.json();
    const select = document.getElementById("event-sport");
    sports.forEach(s => {
        const opt = document.createElement("option");
        opt.value = s;
        opt.textContent = s;
        select.appendChild(opt);
    });
}

async function loadParticipantsBySport() {
    const sport = document.getElementById("event-sport").value;
    const res = await fetch(`/api/participant/by-sport/${sport}`);
    const data = await res.json();

    const p1 = document.getElementById("participant-1");
    const p2 = document.getElementById("participant-2");
    p1.innerHTML = "<option disabled selected>Alege participantul 1</option>";
    p2.innerHTML = "<option disabled selected>Alege participantul 2</option>";

    data.forEach(p => {
        p1.innerHTML += `<option value="${p.name}">${p.name}</option>`;
        p2.innerHTML += `<option value="${p.name}">${p.name}</option>`;
    });
}

function filterParticipant1() {
    const p2Value = document.getElementById("participant-2").value;
    const p1 = document.getElementById("participant-1");

    Array.from(p1.options).forEach(opt => {
        opt.disabled = false; 
        if (opt.value === p2Value && p2Value !== "") {
            opt.disabled = true;
        }
    });

    
    if (p1.value === p2Value) {
        p1.value = "";
    }
}

function filterParticipant2() {
    const p1Value = document.getElementById("participant-1").value;
    const p2 = document.getElementById("participant-2");

    Array.from(p2.options).forEach(opt => {
        opt.disabled = false;
        if (opt.value === p1Value && p1Value !== "") {
            opt.disabled = true;
        }
    });

    if (p2.value === p1Value) {
        p2.value = "";
    }
}


async function loadLocations() {
    const res = await fetch("/api/location");
    const data = await res.json();
    const locSel = document.getElementById("event-location");
    locSel.innerHTML = "<option disabled selected>Alege o locație</option>";

    data.forEach(loc => {
        const opt = document.createElement("option");
        opt.value = loc.locationId; // trimitem ID-ul, nu numele!
        opt.textContent = loc.locationName;
        opt.setAttribute("data-name", loc.locationName); // 🔥 aici e cheia
        locSel.appendChild(opt);
    });

    locSel.onchange = async () => {
        const locationId = locSel.value;
        const res = await fetch(`/api/subsector/by-location/${locationId}`);
        if (!res.ok) {
            showToast("Eroare la încărcarea subsectoarelor", "error");
            return;
        }

        const data = await res.json();
        const container = document.getElementById("subsector-list");
        container.innerHTML = "";

        data.forEach(ss => {
            const row = document.createElement("div");
            row.className = "row align-items-center mb-2";
            row.innerHTML = `
                <div class="col-5">
                    <input type="checkbox" data-subsector-id="${ss.subsectorId}" checked />
                    <label class="ms-1">${ss.sectorName} - ${ss.subsectorName}</label>
                </div>
                <div class="col-4">
                <input type="number" class="form-control" data-subsector-price placeholder="Preț" min="1" value="1" />
                </div>
            `;
            container.appendChild(row);
        });
    };
}


async function submitEvent() {
    try {
        const name = document.getElementById("event-name").value.trim();
        const sport = document.getElementById("event-sport").value;
        const p1 = document.getElementById("participant-1").value;
        const p2 = document.getElementById("participant-2").value;
        const date = document.getElementById("event-date").value;
        const image = document.getElementById("event-image").files[0];
        const description = document.getElementById("event-description").value.trim();
        const locSel = document.getElementById("event-location");
        const selectedOption = locSel.options[locSel.selectedIndex];
        const locationName = selectedOption.getAttribute("data-name");


        if (!name || !sport || !p1 || !p2 || !date || !locationName || !description) {
            return showToast("Completează toate câmpurile evenimentului!", "error");
        }

        if (p1 === p2) {
            return showToast("Participanții trebuie să fie diferiți!", "error");
        }

        // Trimite evenimentul
        const formData = new FormData();
        formData.append("EventName", name);
        formData.append("SportType", sport);
        formData.append("Participant1Name", p1);
        formData.append("Participant2Name", p2);
        const rawDate = document.getElementById("event-date").value;
        const jsDate = new Date(rawDate);
        formData.append("EventDate", jsDate.toISOString()); // ← asta e ce vrei

        formData.append("LocationName", locationName);
        formData.append("Description", description);
        if (image) formData.append("ImageUrl", image);

        const eventRes = await fetch("/api/event", {
            method: "POST",
            body: formData
        });

        if (!eventRes.ok) {
            const err = await eventRes.text();
            console.log(" Eroare back-end:", err);
            return showToast(err || "Eroare la creare eveniment", "error");
        }


        if (!eventRes.ok) {
            const err = await eventRes.text();
            return showToast(err || "Eroare la creare eveniment", "error");
        }

        const createdEvent = await eventRes.json();
        const eventId = createdEvent.eventId;

        // Trimite fiecare EventSector
        const subsectorRows = document.querySelectorAll("#subsector-list .row");
        for (const row of subsectorRows) {
            const checkbox = row.querySelector("input[type=checkbox]");
            const priceInput = row.querySelector("input[data-subsector-price]");
            const subsectorId = checkbox.getAttribute("data-subsector-id");
            const isActive = checkbox.checked;
            const price = parseFloat(priceInput.value);

            console.log(" Preț citit pentru subsector", subsectorId, ":", priceInput.value);

            if (isActive &&( (isNaN(price) || price <= 0))) {
                return showToast("Toate subsectoarele active trebuie să aibă preț valid!", "error");
            }


            const eventSector = {
                eventId: eventId,
                subsectorId: parseInt(subsectorId),
                price: price,
                isActive: isActive
            };

            const esRes = await fetch("/api/eventsector", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(eventSector)
            });

            if (!esRes.ok) {
                const err = await esRes.text();
                return showToast(err || `Eroare la salvarea subsectoarelor.`, "error");
            }
        }

        showToast("Evenimentul a fost creat cu succes!", "success");
        setTimeout(() => location.reload(), 1000);

    } catch (err) {
        console.error("Eroare submitEvent:", err);
        showToast("Eroare neașteptată: " + err.message, "error");
    }
}

function previewEventImage(e) {
    const file = e.target.files[0];
    const preview = document.getElementById("eventImagePreview");

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

async function deleteEvent(id) {
    if (!confirm("Ești sigur că vrei să ștergi acest eveniment?")) return;

    try {
        const res = await fetch(`/api/event/${id}`, {
            method: "DELETE"
        });

        if (res.status === 204) {
            showToast("Eveniment șters cu succes!", "success");
            setTimeout(() => location.reload(), 800);
        } else {
            const msg = await res.text();
            showToast(msg || "Nu se poate șterge evenimentul.", "error");
        }
    } catch (err) {
        showToast("Eroare la ștergere eveniment.", "error");
    }
}

async function openEditEventModal(eventId) {
    try {
        const res = await fetch(`/api/event/${eventId}`);
        if (!res.ok) return showToast("Eroare la încărcarea evenimentului", "error");

        const event = await res.json();

        // Populate fixed fields
        document.getElementById("event-name").value = event.eventName;
        document.getElementById("event-sport").innerHTML = `<option selected>${event.sportType}</option>`;
        document.getElementById("participant-1").innerHTML = `<option selected>${event.participant1Name}</option>`;
        document.getElementById("participant-2").innerHTML = `<option selected>${event.participant2Name}</option>`;
        const rawDate = new Date(event.eventDate);
        const localDateString = new Date(rawDate.getTime() - rawDate.getTimezoneOffset() * 60000)
            .toISOString().slice(0, 16);

        document.getElementById("event-date").value = localDateString;


        const preview = document.getElementById("eventImagePreview");

        if (event.imageUrl) {
            preview.src = event.imageUrl;
            preview.classList.remove("d-none");
        } else {
            preview.src = "";
            preview.classList.add("d-none");
        }


        document.getElementById("event-description").value = event.description || "";
        document.getElementById("event-image").value = "";

        // Set readonly for fixed fields
        document.getElementById("event-sport").disabled = true;
        document.getElementById("participant-1").disabled = true;
        document.getElementById("participant-2").disabled = true;
        document.getElementById("event-location").innerHTML = `<option selected>${event.locationName}</option>`;
        document.getElementById("event-location").disabled = true;
        

        // Încarcă eventSectors
        const secRes = await fetch(`/api/eventsector/event/display/${eventId}`);
        const eventSectors = await secRes.json();
        const container = document.getElementById("subsector-list");
        container.innerHTML = "";

        eventSectors.forEach(es => {
            const disabled = es.isActive ? "disabled checked" : "";
            const row = document.createElement("div");
            row.className = "row align-items-center mb-2";
            row.innerHTML = `
                <div class="col-5">
                    <input type="checkbox" data-subsector-id="${es.subsectorId}" ${disabled} />
                    <label class="ms-1">${es.sectorName} - ${es.subsectorName}</label>
                </div>
                <div class="col-4">
                    <input type="number" class="form-control" data-subsector-price placeholder="Preț"
                           value="${es.price}" min="1" ${es.isActive ? "disabled" : ""} />
                </div>
            `;
            container.appendChild(row);
        });

        // Schimbă comportamentul butonului de submit
        const btn = document.querySelector("#createEventModal .modal-footer button.btn-success");
        btn.textContent = "Salvează modificările";
        btn.onclick = () => submitEditEvent(eventId);

        new bootstrap.Modal(document.getElementById("createEventModal")).show();
    } catch (err) {
        console.error("Eroare openEditEventModal:", err);
        showToast("Eroare la deschiderea modalului", "error");
    }
}


async function submitEditEvent(id) {
    try {
        const name = document.getElementById("event-name").value.trim();
        const date = document.getElementById("event-date").value;
        const description = document.getElementById("event-description").value.trim();
        const image = document.getElementById("event-image").files[0];

        if (!name || !date) {
            return showToast("Completează numele și data!", "error");
        }

        const formData = new FormData();
        formData.append("EventName", name);
        formData.append("EventDateTime", date);
        formData.append("Description", description);
        
            const localDate = new Date(date);
            const corrected = new Date(localDate.getTime() - localDate.getTimezoneOffset() * 60000);
            formData.append("EventDate", corrected.toISOString());

        // Adaugă valorile read-only necesare
        formData.append("SportType", document.getElementById("event-sport").value);
        formData.append("Participant1Name", document.getElementById("participant-1").value);
        formData.append("Participant2Name", document.getElementById("participant-2").value);
        formData.append("LocationName", document.getElementById("event-location").value);

        const res = await fetch(`/api/event/${id}`, {
            method: "PUT",
            body: formData
        });

        if (!res.ok) {
            const text = await res.text();
            return showToast(text || "Eroare la salvare eveniment!", "error");
        }

        // Trimit noii EventSectors activați
        const rows = document.querySelectorAll("#subsector-list .row");
        for (const row of rows) {
            const checkbox = row.querySelector("input[type=checkbox]");
            const input = row.querySelector("input[data-subsector-price]");
            const isActive = checkbox.checked && !checkbox.disabled;

            if (!isActive) continue;

            const price = parseFloat(input.value);
            if (isNaN(price) || price <= 0) {
                return showToast("Preț invalid la un sector nou!", "error");
            }

            const payload = {
                eventId: id,
                subsectorId: parseInt(checkbox.getAttribute("data-subsector-id")),
                price: price,
                isActive: true
            };

            const resp = await fetch("/api/eventsector", {
                method: "PUT",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(payload)
            });

            if (!resp.ok) {
                const txt = await resp.text();
                return showToast(txt || "Eroare la salvare subsector nou", "error");
            }
        }

        showToast("Evenimentul a fost actualizat!", "success");
        setTimeout(() => location.reload(), 1000);
    } catch (err) {
        console.error("submitEditEvent err:", err);
        showToast("Eroare neașteptată: " + err.message, "error");
    }
}







