function showToast(message, type = "info") {
    const toast = document.createElement("div");
    toast.className = `custom-toast toast-${type}`;

    
    toast.innerHTML = `
        <span>${message}</span>
        <button class="toast-close" aria-label="Închide">&times;</button>
    `;

    document.body.appendChild(toast);

    
    setTimeout(() => {
        toast.classList.add("visible");
    }, 100);

    
    toast.querySelector(".toast-close").addEventListener("click", () => {
        toast.classList.remove("visible");
        setTimeout(() => toast.remove(), 300);
    });

    
    setTimeout(() => {
        if (toast.parentElement) {
            toast.classList.remove("visible");
            setTimeout(() => toast.remove(), 300);
        }
    }, 4000);
}
