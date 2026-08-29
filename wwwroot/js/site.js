class Base64 {
    static #textEncoder = new TextEncoder();
    static #textDecoder = new TextDecoder();

    static encode = (str) => btoa(String.fromCharCode(...Base64.#textEncoder.encode(str)));
    static decode = (str) => Base64.#textDecoder.decode(Uint8Array.from(atob(str), c => c.charCodeAt(0)));

    static encodeUrl = (str) => this.encode(str).replace(/\+/g, '-').replace(/\//g, '_');
    static decodeUrl = (str) => this.decode(str.replace(/\-/g, '+').replace(/\_/g, '/'));
}

document.addEventListener('submit', e => {
    const form = e.target;
    if (form.id == 'auth-form') {
        e.preventDefault();
        const formData = new FormData(form);
        const login = formData.get("auth-login");
        const password = formData.get("auth-password");

        let errorMessage = "";
        if (login.trim().length === 0) {
            errorMessage += "Логін не може бути порожнім.\n";
        }
        if (login.includes(':')) {
            errorMessage += "Логін не може містити символ ':'.\n";
        }
        if (password.trim().length === 0) {
            errorMessage += "Пароль не може бути порожнім.\n";
        }

        const err = document.getElementById("auth-modal-error");
        if (errorMessage.length > 0) {
            err.innerText = errorMessage;
            err.style.visibility = "visible";
            return;
        }
        else {
            err.innerText = "";
            err.style.visibility = "hidden";
        }

        const userPass = login + ':' + password;
        const credentials = Base64.encode(userPass);

        fetch("/User/BasicAuth", {
            headers: {
                "Authorization": "Basic " + credentials,
            }
        }).then(r => {
            if (r.ok) {
                window.location.reload();
            }
            else {
                return r.text().then(text => {
                    err.innerText = text || "Помилка автентифікації";
                    err.style.visibility = "visible";
                });
            }
        }).catch(error => {
            err.innerText = "Помилка мережі: " + error.message;
            err.style.visibility = "visible";
        });
    }
});
