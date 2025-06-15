console.log("da");
async function loginUser(event) {
    event.preventDefault();

    const email = document.getElementById("email").value;
    const password = document.getElementById("password").value;

    const loginData = { email, password };

    try {
        // Trimitere POST către API-ul de login
        const response = await fetch("http://localhost:5182/api/User/login", {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
            },
            body: JSON.stringify(loginData)
        });

        if (response.ok) {
            const data = await response.json();
            const token = data.token;
            const role = data.role
            console.log("ok");

            localStorage.setItem("jwt", token);


            document.cookie = `jwt=${token}; path=/; samesite=strict`;
            const payload = JSON.parse(atob(token.split('.')[1]));
            
            const userId = payload["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"];
            localStorage.setItem("userId", userId);

            localStorage.removeItem("cartId");

            
            
            if (role === "administrator") {
                window.location.href = "/Admin/AdminPanel";
            } else {
                window.location.href = "/Index";
            }

            
        } else {
            const error = await response.text();
            showError(error);
        }
    } catch (error) {
        showError("A apărut o eroare la autentificare.");
        console.error("Error during login:", error);
    }
}

function showError(message) {
    const errorMessageDiv = document.getElementById("errorMessage");
    errorMessageDiv.innerText = message;
    errorMessageDiv.style.display = "block";
}