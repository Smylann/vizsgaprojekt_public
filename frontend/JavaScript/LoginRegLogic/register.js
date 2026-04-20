import { register } from './api.js';

const form = document.getElementById('register-form');
const usernameInput = document.getElementById('username');
const emailInput = document.getElementById('useremail');
const passwordInput = document.getElementById('password');
const confirmPasswordInput = document.getElementById('confirm-password');
const errorMsg = document.getElementById('error-msg');
const successMsg = document.getElementById('success-msg');
const registerBtn = document.getElementById('register-btn');

function clearMessages() {
    errorMsg?.classList.add('d-none');
    successMsg?.classList.add('d-none');
    if (errorMsg) errorMsg.textContent = '';
    if (successMsg) successMsg.textContent = '';
}

function showError(message) {
    if (!errorMsg) return;
    errorMsg.textContent = message;
    errorMsg.classList.remove('d-none');
}

function showSuccess(message) {
    if (!successMsg) return;
    successMsg.textContent = message;
    successMsg.classList.remove('d-none');
}

form?.addEventListener('submit', async (event) => {
    event.preventDefault();
    clearMessages();

    if (!usernameInput || !emailInput || !passwordInput || !confirmPasswordInput || !registerBtn) {
        showError('Registration form is not available. Please refresh the page.');
        return;
    }

    const username = usernameInput.value.trim();
    const email = emailInput.value.trim();
    const password = passwordInput.value;
    const confirmPassword = confirmPasswordInput.value;

    if (!username || !email || !password) {
        showError('Username, email and password are required.');
        return;
    }

    if (password !== confirmPassword) {
        showError('Passwords do not match.');
        return;
    }

    registerBtn.disabled = true;

    try {
        await register({ username, email, password });

        showSuccess('Registration successful. Redirecting to login...');
        setTimeout(() => { window.location.href = 'login.html'; }, 1200);
    } catch (err) {
        showError(err.message || 'Something went wrong while creating your account.');
    } finally {
        registerBtn.disabled = false;
    }
});
