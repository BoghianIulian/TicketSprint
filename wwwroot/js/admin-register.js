document.addEventListener("DOMContentLoaded", () => {
    const adminFields = [
        {
            id: "adminFirstName",
            validate: val => val.trim().length > 0,
            message: "Prenumele este obligatoriu."
        },
        {
            id: "adminLastName",
            validate: val => val.trim().length > 0,
            message: "Numele este obligatoriu."
        },
        {
            id: "adminAge",
            validate: val => {
                const age = parseInt(val);
                return !isNaN(age) && age >= 13;
            },
            message: "Vârsta trebuie să fie minim 13."
        },
        {
            id: "adminEmail",
            validate: val => /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(val),
            message: "Email invalid."
        },
        {
            id: "adminPassword",
            validate: val => val.length >= 6,
            message: "Parola trebuie să aibă cel puțin 6 caractere."
        }
    ];

    const errorBoxes = {};

    adminFields.forEach(field => {
        const input = document.getElementById(field.id);
        const errorDiv = document.createElement("div");
        errorDiv.className = "text-danger mt-1 small";
        errorDiv.style.display = "none";
        input.parentNode.appendChild(errorDiv);
        errorBoxes[field.id] = errorDiv;

        input.addEventListener("input", () => {
            if (field.validate(input.value)) {
                input.classList.remove("is-invalid");
                input.classList.add("is-valid");
                errorDiv.style.display = "none";
            } else {
                input.classList.remove("is-valid");
                input.classList.add("is-invalid");
                errorDiv.textContent = field.message;
                errorDiv.style.display = "block";
            }
        });

        input.addEventListener("blur", () => {
            if (!field.validate(input.value)) {
                input.classList.add("is-invalid");
                input.classList.remove("is-valid");
                errorDiv.textContent = field.message;
                errorDiv.style.display = "block";
            } else {
                input.classList.remove("is-invalid");
                input.classList.add("is-valid");
                errorDiv.style.display = "none";
            }
        });
    });

    document.getElementById("adminRegisterForm").addEventListener("submit", async e => {
        e.preventDefault();

        let allValid = true;
        adminFields.forEach(field => {
            const input = document.getElementById(field.id);
            if (!field.validate(input.value)) {
                input.classList.add("is-invalid");
                input.classList.remove("is-valid");
                errorBoxes[field.id].textContent = field.message;
                errorBoxes[field.id].style.display = "block";
                allValid = false;
            }
        });

        if (!allValid) return;

        const user = {
            firstName: document.getElementById("adminFirstName").value.trim(),
            lastName: document.getElementById("adminLastName").value.trim(),
            age: parseInt(document.getElementById("adminAge").value),
            email: document.getElementById("adminEmail").value.trim(),
            password: document.getElementById("adminPassword").value
        };

        try {
            const res = await fetch("/api/user/register-admin", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(user)
            });

            const errorBox = document.getElementById("adminRegisterError");

            if (res.ok) {
                showToast("Cont administrator creat cu succes!", "success");

                const modal = bootstrap.Modal.getInstance(document.getElementById("createAdminModal"));
                modal.hide();

                setTimeout(() => location.reload(), 1000);
            } else {
                const err = await res.text();
                errorBox.innerText = err;
                errorBox.style.display = "block";
            }
        } catch (err) {
            const errorBox = document.getElementById("adminRegisterError");
            errorBox.innerText = "A apărut o eroare. Încearcă din nou.";
            errorBox.style.display = "block";
        }
    });
});
