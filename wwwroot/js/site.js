class Base64 {
    static #textEncoder = new TextEncoder();
    static #textDecoder = new TextDecoder();

    static encode = (str) => btoa(String.fromCharCode(...Base64.#textEncoder.encode(str)));
    static decode = (str) => Base64.#textDecoder.decode(Uint8Array.from(atob(str), c => c.charCodeAt(0)));

    static encodeUrl = (str) => this.encode(str).replace(/\+/g, '-').replace(/\//g, '_');
    static decodeUrl = (str) => this.decode(str.replace(/\-/g, '+').replace(/\_/g, '/'));
}

document.addEventListener('DOMContentLoaded', () => {
    const editBtns = document.querySelectorAll('.btn-edit-group');
    const deleteBtns = document.querySelectorAll('.btn-delete-group');
    const cancelEditBtn = document.getElementById('btn-cancel-edit');
    const submitBtn = document.getElementById('btn-submit-group');
    const formTitle = document.getElementById('form-title');
    const groupForm = document.getElementById('admin-add-group');

    const resetGroupForm = () => {
        if (!groupForm) return;
        groupForm.reset();
        const groupIdInput = document.getElementById('group-id');
        if (groupIdInput) groupIdInput.value = '';
        if (formTitle) formTitle.innerText = 'Додати нову товарну групу';
        if (submitBtn) submitBtn.innerHTML = '<i class="bi bi-plus-lg me-1"></i>Додати';
        if (cancelEditBtn) cancelEditBtn.classList.add('d-none');
    };

    editBtns.forEach(btn => {
        btn.addEventListener('click', () => {
            const id = btn.getAttribute('data-id');
            const parentId = btn.getAttribute('data-parent-id');
            const name = btn.getAttribute('data-name');
            const description = btn.getAttribute('data-description');
            const slug = btn.getAttribute('data-slug');
            const hidden = btn.getAttribute('data-hidden');
            const order = btn.getAttribute('data-order');

            document.getElementById('group-id').value = id || '';
            document.getElementById('group-parent').value = parentId || '';
            document.getElementById('group-name').value = name || '';
            document.getElementById('group-description').value = description || '';
            document.getElementById('group-slug').value = slug || '';
            document.getElementById('group-hidden').checked = hidden === '1' || hidden === 1;
            document.getElementById('group-order').value = order || '';

            if (formTitle) formTitle.innerText = 'Редагування товарної групи';
            if (submitBtn) submitBtn.innerHTML = '<i class="bi bi-check-lg me-1"></i>Зберегти';
            if (cancelEditBtn) cancelEditBtn.classList.remove('d-none');

            groupForm.scrollIntoView({ behavior: 'smooth' });
        });
    });

    if (cancelEditBtn) {
        cancelEditBtn.addEventListener('click', resetGroupForm);
    }

    deleteBtns.forEach(btn => {
        btn.addEventListener('click', () => {
            const id = btn.getAttribute('data-id');
            const name = btn.getAttribute('data-name');
            if (confirm(`Ви дійсно бажаєте видалити (перевести у видалені) групу "${name}"?`)) {
                const formData = new FormData();
                formData.append('id', id);
                fetch('/Admin/DeleteGroup', {
                    method: 'POST',
                    body: formData
                }).then(r => {
                    if (r.ok) {
                        window.location.reload();
                    } else {
                        r.text().then(alert);
                    }
                }).catch(err => alert("Помилка при видаленні: " + err.message));
            }
        });
    });
});

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
    else if (form.id == 'admin-add-group') {
        e.preventDefault();
        const formData = new FormData(form);
        fetch("/Admin/AddGroup", {
            method: "POST",
            body: formData
        }).then(r => {
            if (r.ok) {
                window.location.reload();
            }
            else {
                r.text().then(alert);
            }
        });
    }
    else if (form.id == 'admin-add-product') {
        e.preventDefault();
        const formData = new FormData(form);
        fetch("/Admin/AddProduct", {
            method: "POST",
            body: formData
        }).then(r => {
            r.text().then(alert);
        });
    }
});
