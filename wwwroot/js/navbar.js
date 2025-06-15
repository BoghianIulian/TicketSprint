function logoutUser() {
    
    localStorage.removeItem("userId");
    localStorage.removeItem("jwt");
    document.cookie = "jwt=; path=/; expires=Thu, 01 Jan 1970 00:00:00 UTC;";

    
    window.location.href = "/Index";
}

async function openProfile() {
    const userId = localStorage.getItem("userId");
    if (!userId) return;

    const response = await fetch(`/api/user/${userId}`);
    if (!response.ok) {
        alert("Nu am putut încărca profilul.");
        return;
    }

    const user = await response.json();
    document.getElementById("firstName").value = user.firstName;
    document.getElementById("lastName").value = user.lastName;
    document.getElementById("email").value = user.email;
    document.getElementById("age").value = user.age || "";

    const modal = new bootstrap.Modal(document.getElementById('profileModal'));
    modal.show();
}

async function updateProfile() {
    const userId = localStorage.getItem("userId");

    const dto = {
        userId: parseInt(userId),
        firstName: document.getElementById("firstName").value,
        lastName: document.getElementById("lastName").value,
        age: parseInt(document.getElementById("age").value) || null
    };

    const response = await fetch(`/api/user/${userId}`, {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(dto)
    });

    if (response.ok) {
        alert("Profilul a fost actualizat.");
        bootstrap.Modal.getInstance(document.getElementById("profileModal")).hide();
    } else {
        alert("Eroare la salvare.");
    }
}