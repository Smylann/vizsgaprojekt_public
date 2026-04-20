import { getMe, logout, modifyPassword } from './api.js';

// ─── Toast ────────────────────────────────────────────────────────────────────

function showToast(message, type = 'success', duration = 3000) {
    const tc = document.getElementById('toast-container');
    const el = document.createElement('div');
    el.className = `settings-toast ${type}`;
    el.textContent = message;
    tc.appendChild(el);
    setTimeout(() => {
        el.style.opacity = '0';
        setTimeout(() => el.remove(), 400);
    }, duration);
}

// ─── Password strength ────────────────────────────────────────────────────────

function checkStrength(pw) {
    let score = 0;
    if (pw.length >= 8)  score++;
    if (pw.length >= 12) score++;
    if (/[A-Z]/.test(pw)) score++;
    if (/[0-9]/.test(pw)) score++;
    if (/[^A-Za-z0-9]/.test(pw)) score++;
    return score; // 0-5
}

function updateStrengthBar(pw) {
    const bar   = document.getElementById('pw-strength-bar');
    const label = document.getElementById('pw-strength-label');
    if (!pw) { bar.style.width = '0'; label.textContent = ''; return; }

    const score = checkStrength(pw);
    const pct   = (score / 5) * 100;
    const colors = ['#dc3545', '#fd7e14', '#ffc107', '#20c997', '#198754'];
    const labels = ['Very weak', 'Weak', 'Fair', 'Good', 'Strong'];
    bar.style.width      = pct + '%';
    bar.style.background = colors[score - 1] ?? colors[0];
    label.textContent    = labels[score - 1] ?? '';
    label.style.color    = colors[score - 1] ?? colors[0];
}

// ─── Init ─────────────────────────────────────────────────────────────────────

async function init() {
    const loading = document.getElementById('settings-loading');
    const content = document.getElementById('settings-content');

    // ── Auth check
    let user;
    try {
        user = await getMe();
    } catch {
        window.location.href = '/login.html';
        return;
    }

    loading.style.display = 'none';
    content.style.display = '';

    const username = user.UserName ?? user.username ?? '';
    const role     = (user.Role ?? user.role ?? '').trim();

    document.getElementById('settings-username').textContent     = username;
    document.getElementById('navbar-username-top').textContent   = username;
    document.getElementById('navbar-username-top').style.display = '';

    // Show admin card only for admins
    if (role.toLowerCase() === 'admin') {
        document.getElementById('admin-access-card').style.display = '';
    }

    // ── Logout button
    document.getElementById('logout-btn').style.display = '';
    document.getElementById('logout-btn').addEventListener('click', async () => {
        await logout();
        window.location.href = '/login.html';
    });

    // ── Password strength meter
    document.getElementById('new-pw').addEventListener('input', function () {
        updateStrengthBar(this.value);
        // Clear mismatch error as user types
        const confirm = document.getElementById('confirm-pw').value;
        if (confirm) validateMatch();
    });

    document.getElementById('confirm-pw').addEventListener('input', validateMatch);

    function validateMatch() {
        const newPw     = document.getElementById('new-pw').value;
        const confirmPw = document.getElementById('confirm-pw').value;
        const err       = document.getElementById('confirm-pw-error');
        if (confirmPw && newPw !== confirmPw) {
            err.textContent    = 'Passwords do not match.';
            err.style.display  = '';
        } else {
            err.style.display  = 'none';
        }
    }

    // ── Change password form
    document.getElementById('change-pw-form').addEventListener('submit', async (e) => {
        e.preventDefault();

        const currentPw = document.getElementById('current-pw').value;
        const newPw     = document.getElementById('new-pw').value;
        const confirmPw = document.getElementById('confirm-pw').value;
        const errEl     = document.getElementById('pw-form-error');
        const okEl      = document.getElementById('pw-form-success');
        const btn       = document.getElementById('pw-submit-btn');

        errEl.style.display = 'none';
        okEl.style.display  = 'none';

        if (!currentPw) {
            errEl.textContent  = 'Please enter your current password.';
            errEl.style.display = '';
            return;
        }
        if (!newPw || newPw.length < 6) {
            errEl.textContent  = 'New password must be at least 6 characters.';
            errEl.style.display = '';
            return;
        }
        if (newPw !== confirmPw) {
            errEl.textContent  = 'New passwords do not match.';
            errEl.style.display = '';
            return;
        }

        btn.disabled     = true;
        btn.textContent  = 'Saving…';

        try {
            await modifyPassword(currentPw, newPw);
            okEl.textContent  = '✓ Password changed successfully.';
            okEl.style.display = '';
            // Clear the form
            document.getElementById('change-pw-form').reset();
            document.getElementById('pw-strength-bar').style.width = '0';
            document.getElementById('pw-strength-label').textContent = '';
            showToast('Password changed successfully.');
        } catch (err) {
            const msg = err?.body?.message ?? err?.body?.title ?? null;
            errEl.textContent  = msg || (err?.status === 400 ? 'Current password is incorrect.' : 'Failed to change password.');
            errEl.style.display = '';
            showToast(errEl.textContent, 'danger');
        } finally {
            btn.disabled    = false;
            btn.textContent = 'Save New Password';
        }
    });
}

if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', init);
} else {
    init();
}
