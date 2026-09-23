// ========================================
// EMPLEADOS JAVASCRIPT
// ========================================

document.addEventListener('DOMContentLoaded', function () {
    document.querySelectorAll('.password-toggle').forEach(function (btn) {
        btn.addEventListener('click', function () {
            const input = document.getElementById(this.getAttribute('data-target'));
            if (!input) return;
            const showing = this.classList.toggle('showing');
            input.type = showing ? 'text' : 'password';
        });
    });
});