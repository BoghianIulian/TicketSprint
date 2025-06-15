document.addEventListener("DOMContentLoaded", () => {
    const fields = [
        {
            id: "firstName",
            validate: val => val.trim().length > 0,
            message: "Prenumele este obligatoriu."
        },
        {
            id: "lastName",
            validate: val => val.trim().length > 0,
            message: "Numele este obligatoriu."
        },
        {
            id: "age",
            validate: val => {
                const age = parseInt(val);
                return !isNaN(age) && age >= 13;
            },
            message: "Vârsta trebuie să fie minim 13."
        },
        {
            id: "email",
            validate: val => /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(val),
            message: "Email invalid."
        },
        {
            id: "password",
            validate: val => val.length >= 6,
            message: "Parola trebuie să aibă cel puțin 6 caractere."
        }
    ];

    const errorBoxes = {};

    fields.forEach(field => {
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

    document.getElementById("registerForm").addEventListener("submit", async (e) => {
        e.preventDefault();

        // verifică dacă toate câmpurile sunt valide
        let allValid = true;
        fields.forEach(field => {
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

        // construim obiectul user
        const user = {
            firstName: document.getElementById("firstName").value.trim(),
            lastName: document.getElementById("lastName").value.trim(),
            age: parseInt(document.getElementById("age").value),
            email: document.getElementById("email").value.trim(),
            password: document.getElementById("password").value
        };

        try {
            const response = await fetch("http://localhost:5182/api/User/register", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                },
                body: JSON.stringify(user)
            });

            const errorBox = document.getElementById("registerError");

            if (response.ok) {
                alert("Cont creat cu succes!");
                window.location.href = "/Login";
            } else {
                const error = await response.text();
                errorBox.innerText = error;
                errorBox.style.display = "block";
            }
        } catch (err) {
            console.error("Eroare rețea:", err);
            const errorBox = document.getElementById("registerError");
            errorBox.innerText = "A apărut o eroare. Încearcă din nou.";
            errorBox.style.display = "block";
        }
    });
});
