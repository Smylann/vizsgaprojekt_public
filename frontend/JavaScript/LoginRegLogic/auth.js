import { getMe, login, logout } from './api.js';

let currentUser = null;

export async function initAuth() {
    try {
        currentUser = await getMe();
        onLoggedIn(currentUser);
    } catch (e) {
        if (e.status === 401) onLoggedOut();
    }
    return currentUser;
}

export function getUser() { return currentUser; }
export function isLoggedIn() { return currentUser !== null; }

function onLoggedIn(user) {
    // Show username in navbar, hide login button, show logout
    document.querySelectorAll('[data-show-when="authed"]').forEach(el => el.style.display = '');
    document.querySelectorAll('[data-show-when="guest"]').forEach(el => el.style.display = 'none');
    document.querySelectorAll('#navbar-username, #navbar-username-top, #mobile-username').forEach(el => el.textContent = user.username ?? user.UserName ?? '');
}

function onLoggedOut() {
    document.querySelectorAll('[data-show-when="authed"]').forEach(el => el.style.display = 'none');
    document.querySelectorAll('[data-show-when="guest"]').forEach(el => el.style.display = '');
}

// Attach logout button handler
document.getElementById('logout-btn')?.addEventListener('click', async () => {
    await logout();
    currentUser = null;
    onLoggedOut();
    window.location.href = '/login.html';
});