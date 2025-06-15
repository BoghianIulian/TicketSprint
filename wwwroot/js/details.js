document.addEventListener("DOMContentLoaded", async () => {
    const icons = document.querySelectorAll(".favorite-icon");

    for (const icon of icons) {
        const participantId = icon.dataset.participantId;

        try {
            const res = await fetch(`/api/favorite/exists?participantId=${participantId}`);
            const isFavorite = await res.json();

            icon.classList.remove("bi-star", "bi-star-fill");
            icon.classList.add(isFavorite ? "bi-star-fill" : "bi-star");

            icon.dataset.favorite = isFavorite;
        } catch (err) {
            console.error("Eroare la verificarea favoritei:", err);
        }

        icon.addEventListener("click", async () => {
            const participantId = icon.dataset.participantId;
            const isFavorite = icon.dataset.favorite === "true";

            try {
                if (!isFavorite) {
                    const res = await fetch("/api/favorite", {
                        method: "POST",
                        headers: { "Content-Type": "application/json" },
                        body: JSON.stringify({ participantId: parseInt(participantId) })
                    });

                    if (res.ok) {
                        icon.classList.remove("bi-star");
                        icon.classList.add("bi-star-fill");
                        icon.dataset.favorite = "true";
                        showToast("Adăugat la favorite", "success");
                    } else {
                        showToast("Eroare la adăugare în favorite", "error");
                    }
                } else {
                    // Obținem ID-ul favoritei 
                    const token = getCookie('jwt');


                    const res = await fetch(`/api/favorite/by-user-participant?participantId=${participantId}`, {
                        method: "GET",
                        headers: {
                            'Authorization': 'Bearer ' + (token ?? '')
                        }
                    });
                    
                    console.log("➡️ Click pe stea detectat pentru participantId:", participantId);

                    const favorite = await res.json();
                    console.log("➡️ Răspuns de la /by-user-participant:", favorite);

                    if (favorite && favorite.favoriteId) {
                        const response = await fetch(`/api/favorite/${favorite.favoriteId}`, {
                            method: 'DELETE',
                            headers: {
                                'Authorization': 'Bearer ' + (token ?? '')
                            }
                        });

                        if (response.ok) {
                            icon.classList.remove("bi-star-fill");
                            icon.classList.add("bi-star");
                            icon.dataset.favorite = "false";
                            showToast("Șters din favorite", "info");
                        } else {
                            const error = await response.text();
                            console.error("Eroare la ștergere:", error);
                            showToast("Eroare la ștergere din favorite", "error");
                        }
                    }
                }
            } catch (err) {
                console.error("Eroare la update favorite:", err);
                showToast("Eroare la comunicarea cu serverul", "error");
            }
        });
    }
});

function getCookie(name) {
    const value = `; ${document.cookie}`;
    const parts = value.split(`; ${name}=`);
    if (parts.length === 2) return parts.pop().split(';').shift();
}
