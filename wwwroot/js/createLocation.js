let sectorIndex = 0;
let subsectorIndexMap = {};

function addSector() {
    const container = document.getElementById("sectors-container");

    const sectorId = `sector-${sectorIndex}`;
    subsectorIndexMap[sectorId] = 0;

    const sectorHTML = `
    <div class="border rounded p-3 mb-4" id="${sectorId}-container">
        <div class="d-flex justify-content-between align-items-center">
            <h6 class="fw-bold mb-2"> Sector ${sectorIndex + 1}</h6>
            <button class="btn btn-sm btn-outline-danger" onclick="removeSector('${sectorId}')">🗑️ Șterge sector</button>
        </div>
        <div class="row g-2 mb-3">
            <div class="col-md-6">
                <label class="form-label">Nume sector</label>
                <input type="text" class="form-control sector-name" data-sector-id="${sectorId}" />
            </div>
            <div class="col-md-6 text-end">
                <button class="btn btn-outline-secondary mt-4" onclick="addSubsector('${sectorId}')">➕ Adaugă subsector</button>
            </div>
        </div>
        <div id="${sectorId}-subsectors"></div>
    </div>`;

    container.insertAdjacentHTML("beforeend", sectorHTML);
    addSubsector(sectorId); // adaugă automat un subsector
    sectorIndex++;
}

function addSubsector(sectorId) {
    const container = document.getElementById(`${sectorId}-subsectors`);
    const subIndex = subsectorIndexMap[sectorId]++;

    const newSubsector = createSubsectorElement(); // fără parametri = default values
    container.appendChild(newSubsector);

    calculateAvailableSeats(); // recalculează imediat după adăugare
}


function createSubsectorElement(subsector = {}) {
    const { subsectorName = "", rows = 1, seatsPerRow = 1, subsectorId = null } = subsector;

    const wrapper = document.createElement("div");
    wrapper.className = "row g-2 align-items-end mb-2";
    wrapper.setAttribute("data-subsector", "");
    if (subsectorId) wrapper.setAttribute("data-subsector-backend-id", subsectorId);

    wrapper.innerHTML = `
        <div class="col-md-4">
            <label class="form-label">Nume subsector</label>
            <input type="text" class="form-control" data-subsector-name value="${subsectorName}" />
        </div>
        <div class="col-md-3">
            <label class="form-label">Rânduri</label>
            <input type="number" class="form-control" data-subsector-rows value="${rows}" min="1" />
        </div>
        <div class="col-md-3">
            <label class="form-label">Locuri / rând</label>
            <input type="number" class="form-control" data-subsector-seats value="${seatsPerRow}" min="1" />
        </div>
        <div class="col-md-2 text-end">
            <button class="btn btn-sm btn-outline-danger btn-delete-subsector" onclick="removeSubsector(this)">🗑️</button>
        </div>`;

    // 🎯 atașează recalcularea automată
    wrapper.querySelector("[data-subsector-rows]").addEventListener("input", calculateAvailableSeats);
    wrapper.querySelector("[data-subsector-seats]").addEventListener("input", calculateAvailableSeats);

    return wrapper;
}


function removeSector(sectorId) {
    const el = document.getElementById(`${sectorId}-container`);
    if (el) el.remove();
    calculateAvailableSeats();
}

function removeSubsector(btn) {
    const row = btn.closest("[data-subsector]");
    if (row) row.remove();
    calculateAvailableSeats();
}


function calculateAvailableSeats() {
    const cap = parseInt(document.getElementById("loc-capacity").value) || 0;
    let used = 0;

    document.querySelectorAll("[data-subsector]").forEach(row => {
        const rows = parseInt(row.querySelector("[data-subsector-rows]").value) || 0;
        const seats = parseInt(row.querySelector("[data-subsector-seats]").value) || 0;
        used += rows * seats;
    });

    const available = Math.max(0, cap - used);
    document.getElementById("available-capacity").innerText = available;
}


async function submitLocation() {
    let locationId = null;
    const createdSectorIds = [];
    const createdSubsectorIds = [];

    try {
        //   Validare UI completă
        const name = document.getElementById("loc-name").value.trim();
        const type = document.querySelector("input[name='loc-type']:checked")?.value;
        const city = document.getElementById("loc-city").value;
        const capacity = parseInt(document.getElementById("loc-capacity").value);

        if (!name || !type || !city || !capacity || capacity <= 0)
            return showToast("Completează toate câmpurile locației corect!", "error");

        const sectors = document.querySelectorAll("[id^=sector-][id$=-container]");
        if (sectors.length === 0)
            return showToast("Adaugă cel puțin un sector.", "error");

        //   Creează locația
        const locationRes = await fetch("/api/location", {
            method: "POST",
            headers: { "Content-Type": "application/json; charset=UTF-8" },
            body: JSON.stringify({ locationName: name, locationType: type, city, capacity })
        });

        if (!locationRes.ok) throw new Error(await locationRes.text());

        const locationData = await locationRes.json();
        locationId = locationData.locationId;

        //   Creează fiecare sector și subsector
        for (const sectorContainer of sectors) {
            const sectorName = sectorContainer.querySelector(".sector-name").value.trim();
            if (!sectorName) throw new Error("Toate sectoarele trebuie să aibă nume.");

            const sectorRes = await fetch("/api/sector", {
                method: "POST",
                headers: { "Content-Type": "application/json; charset=UTF-8" },
                body: JSON.stringify({ sectorName, locationName: name })
            });

            if (!sectorRes.ok) throw new Error(await sectorRes.text());

            const sectorData = await sectorRes.json();
            const sectorId = sectorData.sectorId;
            createdSectorIds.push(sectorId);

            const subsectors = sectorContainer.querySelectorAll("[data-subsector]");
            if (subsectors.length === 0)
                throw new Error(`Sectorul "${sectorName}" nu are niciun subsector.`);

 
            const seenNames = new Set();
            for (const row of subsectors) {
                const subName = row.querySelector("[data-subsector-name]").value.trim().toLowerCase();
                if (seenNames.has(subName)) {
                    throw new Error(`Sectorul "${sectorName}" are două subsectoare cu același nume: "${subName}"`);                }
                seenNames.add(subName);
            }


            for (const row of subsectors) {
                const subName = row.querySelector("[data-subsector-name]").value.trim();
                const rows = parseInt(row.querySelector("[data-subsector-rows]").value);
                const seats = parseInt(row.querySelector("[data-subsector-seats]").value);

                if (!subName || rows <= 0 || seats <= 0)
                    throw new Error(`Date incorecte la un subsector din sectorul ${sectorName}.`);

                const subRes = await fetch("/api/subsector", {
                    method: "POST",
                    headers: { "Content-Type": "application/json; charset=UTF-8" },
                    body: JSON.stringify({ subsectorName: subName, sectorId, rows, seatsPerRow: seats })
                });

                if (!subRes.ok) throw new Error(await subRes.text());

                const subData = await subRes.json();
                createdSubsectorIds.push(subData.subsectorId);
            }
        }

        showToast("Locația a fost creată cu succes!", "success");
        setTimeout(() => location.reload(), 1000);

    } catch (err) {
        
        for (const subId of createdSubsectorIds.reverse()) {
            await fetch(`/api/subsector/${subId}`, { method: "DELETE" });
        }

        for (const secId of createdSectorIds.reverse()) {
            await fetch(`/api/sector/${secId}`, { method: "DELETE" });
        }

        if (locationId) {
            await fetch(`/api/location/${locationId}`, { method: "DELETE" });
        }

        console.error("Eroare submitLocation:", err);
        showToast("Eroare neașteptată: " + err.message, "error");
    }
}


async function updateLocation() {
    try {
        const idInput = document.getElementById("loc-id");
        if (!idInput) return showToast("ID-ul locației nu a fost găsit!", "error");

        const locationId = parseInt(idInput.value);
        const name = document.getElementById("loc-name").value.trim();
        const type = document.querySelector("input[name='loc-type']:checked")?.value;
        const city = document.getElementById("loc-city").value;
        const capacity = parseInt(document.getElementById("loc-capacity").value);

        if (!name || !type || !city || !capacity || capacity <= 0)
            return showToast("Completează toate câmpurile locației corect!", "error");

        const locationPayload = { locationId, locationName: name, locationType: type, city, capacity };

        const locationRes = await fetch(`/api/location/${locationId}`, {
            method: "PUT",
            headers: { "Content-Type": "application/json; charset=UTF-8" },
            body: JSON.stringify(locationPayload)
        });

        if (!locationRes.ok) {
            const err = await locationRes.text();
            return showToast(err || "Eroare la actualizarea locației.", "error");
        }

        const initialSectorsRes = await fetch(`/api/sector/by-location/${locationId}`);
        const initialSectors = await initialSectorsRes.json();
        const initialSectorIds = initialSectors.map(s => s.sectorId);

        const initialSubsectorsRes = await fetch(`/api/subsector/by-location/${locationId}`);
        const initialSubsectors = await initialSubsectorsRes.json();
        let initialSubsectorIds = initialSubsectors.map(ss => ss.subsectorId);

        const usedSectorIds = new Set();
        const usedSubsectorIds = new Set();

        const sectors = document.querySelectorAll("[id^=sector-][id$=-container]");
        if (sectors.length === 0)
            return showToast("Adaugă cel puțin un sector.", "error");

        for (const sectorContainer of sectors) {
            const sectorName = sectorContainer.querySelector(".sector-name").value.trim();
            const backendSectorId = sectorContainer.getAttribute("data-sector-backend-id");

            if (!sectorName) return showToast("Toate sectoarele trebuie să aibă nume.", "error");

            const sectorPayload = backendSectorId
                ? { sectorId: parseInt(backendSectorId), sectorName, locationId }
                : { sectorName, locationName: name };

            const sectorRes = await fetch(backendSectorId ? `/api/sector/${backendSectorId}` : "/api/sector", {
                method: backendSectorId ? "PUT" : "POST",
                headers: { "Content-Type": "application/json; charset=UTF-8" },
                body: JSON.stringify(sectorPayload)
            });

            if (!sectorRes.ok) {
                const err = await sectorRes.text();
                return showToast(err || `Eroare la salvare sector: ${sectorName}`, "error");
            }

            let finalSectorId = backendSectorId;
            if (!backendSectorId && sectorRes.status !== 204) {
                const sectorData = await sectorRes.json();
                finalSectorId = sectorData.sectorId;
            }

            usedSectorIds.add(parseInt(finalSectorId));

            const subsectors = sectorContainer.querySelectorAll("[data-subsector]");
            if (subsectors.length === 0)
                throw new Error(`Sectorul "${sectorName}" nu are niciun subsector.`);


            const subNamesSet = new Set();
            for (const row of subsectors) {
                const subName = row.querySelector("[data-subsector-name]").value.trim();
                if (subNamesSet.has(subName.toLowerCase())) {
                    return showToast(`Sectorul "${sectorName}" are două subsectoare cu același nume: "${subName}"`, "error");
                }
                subNamesSet.add(subName.toLowerCase());
            }


            for (const row of subsectors) {
                const subName = row.querySelector("[data-subsector-name]").value.trim();
                const rows = parseInt(row.querySelector("[data-subsector-rows]").value);
                const seats = parseInt(row.querySelector("[data-subsector-seats]").value);
                const backendSubId = row.getAttribute("data-subsector-backend-id");

                if (!subName || rows <= 0 || seats <= 0)
                    return showToast(`Date incorecte la un subsector din sectorul ${sectorName}.`, "error");

                const isLocked = row.dataset.locked === "true";

                if (isLocked && backendSubId) {
                    const initial = initialSubsectors.find(ss => ss.subsectorId == backendSubId);
                    if (initial) {
                        const rowsChanged = rows !== initial.rows;
                        const seatsChanged = seats !== initial.seatsPerRow;
                        if (rowsChanged || seatsChanged) {
                            showToast(`Subsectorul "${subName}" este blocat. Nu poți modifica rândurile sau locurile.`, "error");
                            continue;
                        }
                    }
                }

                const subPayload = {
                    subsectorName: subName,
                    sectorId: parseInt(finalSectorId),
                    rows,
                    seatsPerRow: seats
                };

                if (backendSubId) subPayload.subsectorId = parseInt(backendSubId);

                const subRes = await fetch(backendSubId ? `/api/subsector/${backendSubId}` : "/api/subsector", {
                    method: backendSubId ? "PUT" : "POST",
                    headers: { "Content-Type": "application/json; charset=UTF-8" },
                    body: JSON.stringify(subPayload)
                });

                if (!subRes.ok) {
                    const err = await subRes.text();
                    return showToast(err || `Eroare la salvare subsector în ${sectorName}`, "error");
                }

                if (backendSubId) usedSubsectorIds.add(parseInt(backendSubId));
            }
        }

        //  Ștergere subsectoare care nu mai există
        for (const oldId of initialSubsectorIds) {
            if (!usedSubsectorIds.has(oldId)) {
                const el = document.querySelector(`[data-subsector-backend-id="${oldId}"]`);
                if (el?.dataset.locked === "true") continue;

                const res = await fetch(`/api/subsector/${oldId}`, { method: "DELETE" });
                if (!res.ok) {
                    const err = await res.text();
                    return showToast(err || "Nu s-a putut șterge un subsector.", "error");
                }
            }
        }

        //  Ștergere sectoare care nu mai există
        for (const oldId of initialSectorIds) {
            if (!usedSectorIds.has(oldId)) {
                const res = await fetch(`/api/sector/${oldId}`, { method: "DELETE" });
                if (!res.ok) {
                    const err = await res.text();
                    return showToast(err || "Nu s-a putut șterge un sector.", "error");
                }

                // eliminăm subsectoarele asociate acestui sector deja șters în cascada
                const selector = `[data-sector-backend-id="${oldId}"] [data-subsector-backend-id]`;
                const subsectorElems = document.querySelectorAll(selector);
                const idsToRemove = Array.from(subsectorElems)
                    .map(el => parseInt(el.getAttribute("data-subsector-backend-id")))
                    .filter(id => !isNaN(id));

                initialSubsectorIds = initialSubsectorIds.filter(id => !idsToRemove.includes(id));

            }
        }

        

        showToast("Modificările au fost salvate cu succes!", "success");
        setTimeout(() => location.reload(), 1000);

    } catch (err) {
        console.error("🛑 Eroare updateLocation:", err);
        showToast("Eroare neașteptată: " + err.message, "error");
    }
}






function openCreateLocationModal() {
    document.getElementById("loc-id")?.remove?.();

    // Golește câmpuri
    document.getElementById("loc-name").value = "";
    document.getElementById("tip-stadion").checked = true;
    document.getElementById("loc-city").value = "Cluj-Napoca";
    document.getElementById("loc-capacity").value = 10000;

    // Resetează sectoare
    const container = document.getElementById("sectors-container");
    container.innerHTML = "";
    sectorIndex = 0;
    subsectorIndexMap = {};
    addSector(); // adaugă un sector implicit

    // Setează text + funcție buton
    const submitBtn = document.getElementById("submitLocationBtn");
    submitBtn.textContent = "Creează locația";
    submitBtn.onclick = submitLocation;

    // Afișează modalul
    const modal = new bootstrap.Modal(document.getElementById("createLocationModal"));
    modal.show();
}



async function openEditLocationModal(id) {
    try {
        // 1. Obține locația
        const locRes = await fetch(`/api/location/${id}`);
        if (!locRes.ok) throw new Error("Eroare la încărcarea locației");
        const loc = await locRes.json();

        // 2. Obține sectoarele și subsectoarele
        const [sectorRes, subRes] = await Promise.all([
            fetch(`/api/sector/by-location/${id}`),
            fetch(`/api/subsector/by-location/${id}`)
        ]);

        if (!sectorRes.ok || !subRes.ok)
            throw new Error("Eroare la încărcarea sectoarelor/subsectoarelor");

        const sectors = await sectorRes.json();
        const allSubsectors = await subRes.json();

        // 3. Populează formularul locației
        document.getElementById("loc-name").value = loc.locationName;
        document.querySelector(`[name="loc-type"][value="${loc.locationType}"]`).checked = true;
        document.getElementById("loc-city").value = loc.city;
        document.getElementById("loc-capacity").value = loc.capacity;

        document.getElementById("loc-id")?.remove?.();
        const hiddenId = document.createElement("input");
        hiddenId.type = "hidden";
        hiddenId.id = "loc-id";
        hiddenId.value = loc.locationId;
        document.getElementById("createLocationModal").appendChild(hiddenId);

        document.getElementById("submitLocationBtn").textContent = "Salvează modificările";
        document.getElementById("submitLocationBtn").setAttribute("data-mode", "edit");
        document.getElementById("submitLocationBtn").onclick = updateLocation;

        // 4. Populează sectoarele și subsectoarele
        const container = document.getElementById("sectors-container");
        container.innerHTML = "";
        sectorIndex = 0;
        subsectorIndexMap = {};

        for (const sector of sectors) {
            const sectorId = `sector-${sectorIndex}`;
            subsectorIndexMap[sectorId] = 0;

            const sectorHTML = `
                <div class="border rounded p-3 mb-4" id="${sectorId}-container" data-sector-backend-id="${sector.sectorId}">
                  <div class="d-flex justify-content-between align-items-center">
                      <h6 class="fw-bold mb-2"> Sector ${sectorIndex + 1}</h6>
                      <button class="btn btn-sm btn-outline-danger btn-delete-sector" onclick="removeSector('${sectorId}')">🗑️ Șterge sector</button>
                  </div>
                  <div class="row g-2 mb-3">
                      <div class="col-md-6">
                          <label class="form-label">Nume sector</label>
                          <input type="text" class="form-control sector-name" value="${sector.sectorName}" data-sector-id="${sectorId}" />
                      </div>
                      <div class="col-md-6 text-end">
                          <button class="btn btn-outline-secondary mt-4" onclick="addSubsector('${sectorId}')">➕ Adaugă subsector</button>
                      </div>
                  </div>
                  <div id="${sectorId}-subsectors"></div>
                </div>`;

            container.insertAdjacentHTML("beforeend", sectorHTML);
            const sectorElem = document.getElementById(`${sectorId}-container`);

            // ✅ verificăm dacă sectorul e blocat
            await checkAndLockElement("sector", sector.sectorId, sectorElem);

            const subContainer = document.getElementById(`${sectorId}-subsectors`);
            const subsectors = allSubsectors.filter(s => s.sectorId === sector.sectorId);

            for (const ss of subsectors) {
                const el = createSubsectorElement({
                    subsectorName: ss.subsectorName,
                    rows: ss.rows,
                    seatsPerRow: ss.seatsPerRow,
                    subsectorId: ss.subsectorId
                });

                subContainer.appendChild(el);

                // ✅ verificăm dacă subsectorul e blocat
                await checkAndLockElement("subsector", ss.subsectorId, el);
            }

            sectorIndex++;
        }

        calculateAvailableSeats();
        new bootstrap.Modal(document.getElementById("createLocationModal")).show();
    } catch (err) {
        showToast("Eroare la încărcarea datelor locației", "error");
        console.error(err);
    }
}


async function checkAndLockElement(type, id, element) {
    const res = await fetch(`/api/${type}/locked/${id}`);
    if (!res.ok) return;

    const data = await res.json();
    if (data.locked) {
        element.dataset.locked = "true";

        if (type === "sector") {
            const deleteBtn = element.querySelector("button.btn-outline-danger");
            if (deleteBtn) deleteBtn.style.display = "none";
        }

        if (type === "subsector") {
            element.querySelector("[data-subsector-rows]")?.setAttribute("disabled", "true");
            element.querySelector("[data-subsector-seats]")?.setAttribute("disabled", "true");

            const deleteBtn = element.querySelector(".btn-delete-subsector");
            if (deleteBtn) deleteBtn.style.display = "none";
        }
    }
}



