// ========================================
// LOGIN JAVASCRIPT
// ========================================

document.addEventListener('DOMContentLoaded', function() {
    const loginForm = document.getElementById('loginForm');
    const usuarioInput = document.getElementById('usuario');
    const passwordInput = document.getElementById('password');
    const loginBtn = document.querySelector('.login-btn');
    const passwordToggle = document.getElementById('passwordToggle');

    if (passwordToggle) {
        passwordToggle.addEventListener('click', function() {
            const isShowing = this.classList.toggle('showing');
            passwordInput.type = isShowing ? 'text' : 'password';
        });
    }

    if (loginForm) {
        // Handle form submission
        loginForm.addEventListener('submit', function(e) {
            e.preventDefault();

            const usuario = usuarioInput.value.trim();
            const password = passwordInput.value.trim();

            // Validación básica
            if (!usuario || !password) {
                showAlert('Por favor complete todos los campos', 'warning');
                return;
            }

            // Limpiar errores previos
            usuarioInput.classList.remove('is-invalid');
            passwordInput.classList.remove('is-invalid');

            // Cambiar estado del botón
            loginBtn.disabled = true;
            const originalText = loginBtn.textContent;
            loginBtn.textContent = 'Procesando...';

            // Hacer solicitud POST al servidor
            fetch('/Auth/Login', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded'
                },
                body: new URLSearchParams({
                    'Usuario': usuario,
                    'Password': password,
                    'Recuerdame': false,
                    '__RequestVerificationToken': getAntiForgeryToken()
                })
            })
            .then(async response => {
                const data = await response.json().catch(() => null);
                if (response.ok) {
                    return data;
                }
                throw new Error(data?.mensaje || 'Error en la autenticación');
            })
            .then(data => {
                // Login exitoso
                loginBtn.textContent = '¡Bienvenido!';
                loginBtn.style.background = 'linear-gradient(135deg, #4caf50 0%, #388e3c 100%)';

                showAlert('Login exitoso. Redirigiendo...', 'success');

                setTimeout(() => {
                    // Redirigir según la respuesta del servidor
                    window.location.href = data.redirectUrl || '/Home/Dashboard';
                }, 1500);
            })
            .catch(error => {
                // Login fallido
                console.error('Error:', error);
                loginBtn.disabled = false;
                loginBtn.textContent = originalText;

                showAlert(error.message || 'Error al iniciar sesión', 'danger');

                // Marcar campos como inválidos
                usuarioInput.classList.add('is-invalid');
                passwordInput.classList.add('is-invalid');
            });
        });

        // Clear error styling cuando el usuario empieza a escribir
        usuarioInput.addEventListener('focus', function() {
            this.classList.remove('is-invalid');
        });

        passwordInput.addEventListener('focus', function() {
            this.classList.remove('is-invalid');
        });

        // Enter en cualquier campo envía el formulario
        usuarioInput.addEventListener('keypress', function(e) {
            if (e.key === 'Enter') {
                loginForm.dispatchEvent(new Event('submit'));
            }
        });

        passwordInput.addEventListener('keypress', function(e) {
            if (e.key === 'Enter') {
                loginForm.dispatchEvent(new Event('submit'));
            }
        });
    }
});

// Función para obtener el token CSRF
function getAntiForgeryToken() {
    const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value || '';
    return token;
}

// Función para mostrar alertas
function showAlert(message, type = 'info') {
    const alertDiv = document.createElement('div');
    alertDiv.className = `alert alert-${type} alert-dismissible fade show`;
    alertDiv.role = 'alert';
    alertDiv.style.position = 'fixed';
    alertDiv.style.top = '20px';
    alertDiv.style.right = '20px';
    alertDiv.style.zIndex = '9999';
    alertDiv.style.minWidth = '300px';
    alertDiv.style.boxShadow = '0 4px 12px rgba(0, 0, 0, 0.15)';
    alertDiv.innerHTML = `
        ${message}
        <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
    `;

    document.body.appendChild(alertDiv);

    // Auto-remove después de 5 segundos
    setTimeout(() => {
        if (alertDiv.parentElement) {
            alertDiv.remove();
        }
    }, 5000);
}

// Agregar feedback visual al escribir
document.addEventListener('input', function(e) {
    if (e.target.classList.contains('login-input')) {
        e.target.style.borderColor = '#d4af37';
    }
});

