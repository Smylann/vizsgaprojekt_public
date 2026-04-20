import { getMe } from './api.js';

// ─── API helpers ────────────────────────────────────────────────────────────

const API_BASE = (window.__API_BASE_URL__ ?? '/api').replace(/\/$/, '');

async function apiFetch(path, options = {}) {
    const res = await fetch(`${API_BASE}${path}`, {
        headers: { 'Content-Type': 'application/json', ...options.headers },
        credentials: 'include',
        ...options,
    });
    if (!res.ok) {
        const body = await res.json().catch(() => null);
        throw { status: res.status, body };
    }
    return res.json().catch(() => null);
}

const api = {
    getAllUsers:      ()         => apiFetch('/News/getallusers'),
    getAllPosts:      ()         => apiFetch('/News/getallposts'),
    getAllCats:       ()         => apiFetch('/News/getallcats'),
    getAllReports:    ()         => apiFetch('/News/getallreports'),
    getPostComments: (id)        => apiFetch(`/News/${id}/comments`),

    deleteUser:      (id)        => apiFetch(`/News/delete_users?id=${id}`,    { method: 'DELETE' }),
    deletePost:      (id)        => apiFetch(`/News/delete_posts?id=${id}`,    { method: 'DELETE' }),
    deleteComment:   (id)        => apiFetch(`/News/delete_comments?id=${id}`, { method: 'DELETE' }),
    deleteCat:       (id)        => apiFetch(`/News/delete_category?id=${id}`, { method: 'DELETE' }),

    toggleRole:      (id)        => apiFetch(`/User/rolemodify?userid=${id}`,  { method: 'PUT' }),
    renameUser:      (id, name)  => apiFetch('/News/modify_user', {
        method: 'PUT',
        body: JSON.stringify({ id, name }),
    }),
    createCat:       (name)      => apiFetch('/News/create_category', {
        method: 'POST',
        body: JSON.stringify({ categoryname: name }),
    }),
    resolveReportKeep:   (id)    => apiFetch(`/News/resolve_report_keep?id=${id}`, { method: 'PUT' }),
    resolveReportDelete: (id)    => apiFetch(`/News/resolve_report_delete?id=${id}`, { method: 'PUT' }),
};

// ─── State ───────────────────────────────────────────────────────────────────

let allUsers = [];
let allPosts = [];
let allCats  = [];
let allReports = [];

// ─── Bootstrap modal refs (populated after DOMContentLoaded) ─────────────────

let deleteModal, renameModal;
let pendingDeleteFn = null;

// ─── Toast ───────────────────────────────────────────────────────────────────

function showToast(message, type = 'success', duration = 3000) {
    const tc = document.getElementById('toast-container');
    const el = document.createElement('div');
    el.className = `admin-toast ${type}`;
    el.textContent = message;
    tc.appendChild(el);
    setTimeout(() => {
        el.style.opacity = '0';
        setTimeout(() => el.remove(), 400);
    }, duration);
}

// ─── Confirm-delete helper ────────────────────────────────────────────────────

function confirmDelete(message, fn) {
    document.getElementById('delete-modal-message').textContent = message;
    pendingDeleteFn = fn;
    deleteModal.show();
}

// ─── Overview stats ──────────────────────────────────────────────────────────

function updateStats() {
    document.getElementById('stat-users').textContent  = allUsers.length;
    document.getElementById('stat-posts').textContent  = allPosts.length;
    document.getElementById('stat-cats').textContent   = allCats.length;

    const totalVotes = allPosts.reduce((s, p) => s + (p.votes ?? p.Votes ?? 0), 0);
    document.getElementById('stat-votes').textContent  = totalVotes;

    const admins = allUsers.filter(u => (u.role ?? u.Role) === 'Admin').length;
    document.getElementById('stat-admins').textContent = admins;

    const negativePosts = allPosts.filter(p => (p.votes ?? p.Votes ?? 0) < 0).length;
    document.getElementById('stat-negative-posts').textContent = negativePosts;

    if (allPosts.length > 0) {
        const top = allPosts.reduce((a, b) =>
            (a.votes ?? a.Votes ?? 0) >= (b.votes ?? b.Votes ?? 0) ? a : b
        );
        document.getElementById('stat-top-post-votes').textContent = top.votes ?? top.Votes ?? 0;
        document.getElementById('stat-top-post-title').textContent =
            (top.title ?? top.Title ?? '').substring(0, 50) || '–';
    }
}

// ─── Users tab ───────────────────────────────────────────────────────────────

function renderUsers(users) {
    const tbody = document.getElementById('users-tbody');
    tbody.innerHTML = '';
    if (!users.length) {
        tbody.innerHTML = `<tr><td colspan="4" class="text-center text-muted py-3">No users found.</td></tr>`;
        return;
    }
    for (const u of users) {
        const id       = u.userID ?? u.UserID;
        const username = u.username ?? u.Username;
        const role     = u.role ?? u.Role ?? 'User';
        const isAdmin  = role === 'Admin';

        const tr = document.createElement('tr');
        tr.dataset.userId   = id;
        tr.dataset.username = username.toLowerCase();
        tr.dataset.role     = role.toLowerCase();
        tr.innerHTML = `
          <td class="text-muted">${id}</td>
          <td><strong>${esc(username)}</strong></td>
          <td>
            <span class="badge ${isAdmin ? 'badge-admin' : 'badge-user'} user-role-badge">
              ${esc(role)}
            </span>
          </td>
          <td>
            <div class="d-flex gap-1 flex-wrap">
              <button class="btn btn-outline-warning btn-action btn-toggle-role"
                title="${isAdmin ? 'Demote to User' : 'Promote to Admin'}">
                ${isAdmin ? '↓ Demote' : '↑ Promote'}
              </button>
              <button class="btn btn-outline-info btn-action btn-rename-user" title="Rename">✏ Rename</button>
              <button class="btn btn-outline-danger btn-action btn-delete-user" title="Delete user">🗑 Delete</button>
            </div>
          </td>`;

        tr.querySelector('.btn-toggle-role').addEventListener('click', () => toggleRole(id, tr));
        tr.querySelector('.btn-rename-user').addEventListener('click', () => openRenameModal(id, username));
        tr.querySelector('.btn-delete-user').addEventListener('click', () =>
            confirmDelete(`Delete user "${username}" (ID ${id})? This cannot be undone.`, async () => {
                try {
                    await api.deleteUser(id);
                    allUsers = allUsers.filter(x => (x.userID ?? x.UserID) !== id);
                    tr.remove();
                    updateStats();
                    showToast(`User "${username}" deleted.`);
                } catch (e) {
                    showToast('Failed to delete user: ' + (e.body?.message ?? e.status), 'danger');
                }
            })
        );

        tbody.appendChild(tr);
    }
}

async function toggleRole(id, tr) {
    const badge  = tr.querySelector('.user-role-badge');
    const btn    = tr.querySelector('.btn-toggle-role');
    try {
        await api.toggleRole(id);
        const wasAdmin = badge.textContent.trim() === 'Admin';
        const newRole  = wasAdmin ? 'User' : 'Admin';
        badge.textContent = newRole;
        badge.className = `badge ${newRole === 'Admin' ? 'badge-admin' : 'badge-user'} user-role-badge`;
        btn.textContent = newRole === 'Admin' ? '↓ Demote' : '↑ Promote';
        btn.title = newRole === 'Admin' ? 'Demote to User' : 'Promote to Admin';

        // Update in-memory list
        const u = allUsers.find(x => (x.userID ?? x.UserID) === id);
        if (u) { u.role = newRole; u.Role = newRole; }
        updateStats();
        showToast(`Role changed to ${newRole}.`);
    } catch (e) {
        showToast('Failed to change role: ' + (e.body?.message ?? e.status), 'danger');
    }
}

function openRenameModal(id, currentName) {
    document.getElementById('rename-input').value   = currentName;
    document.getElementById('rename-user-id').value = id;
    renameModal.show();
}

// ─── Posts tab ───────────────────────────────────────────────────────────────

function renderPosts(posts) {
    const tbody = document.getElementById('posts-tbody');
    tbody.innerHTML = '';
    if (!posts.length) {
        tbody.innerHTML = `<tr><td colspan="6" class="text-center text-muted py-3">No posts found.</td></tr>`;
        return;
    }
    for (const p of posts) {
        const id      = p.postID  ?? p.PostID;
        const title   = p.title   ?? p.Title   ?? '(no title)';
        const userId  = p.userID  ?? p.UserID  ?? '?';
        const votes   = p.votes   ?? p.Votes   ?? 0;
        const created = p.created_at ?? p.Created_at;
        const dateStr = created ? new Date(created).toLocaleDateString() : '–';

        const tr = document.createElement('tr');
        tr.dataset.postId    = id;
        tr.dataset.postTitle = title.toLowerCase();
        tr.innerHTML = `
          <td class="text-muted">${id}</td>
          <td><span class="truncate d-inline-block" title="${esc(title)}">${esc(title)}</span></td>
          <td class="text-muted">${userId}</td>
          <td>
            <span class="${votes >= 0 ? 'text-success' : 'text-danger'} fw-semibold">${votes >= 0 ? '+' : ''}${votes}</span>
          </td>
          <td class="text-muted" style="font-size:0.85rem">${dateStr}</td>
          <td>
            <button class="btn btn-outline-danger btn-action btn-delete-post" title="Delete post">🗑 Delete</button>
          </td>`;

        tr.querySelector('.btn-delete-post').addEventListener('click', () =>
            confirmDelete(`Delete post "${title}" (ID ${id})? This cannot be undone.`, async () => {
                try {
                    await api.deletePost(id);
                    allPosts = allPosts.filter(x => (x.postID ?? x.PostID) !== id);
                    tr.remove();
                    // Also remove from comments accordion
                    document.getElementById(`comments-post-${id}`)?.closest('.accordion-item')?.remove();
                    updateStats();
                    showToast(`Post "${title}" deleted.`);
                } catch (e) {
                    showToast('Failed to delete post: ' + (e.body?.message ?? e.status), 'danger');
                }
            })
        );

        tbody.appendChild(tr);
    }
}

// ─── Comments tab ────────────────────────────────────────────────────────────

function buildCommentsAccordion(posts) {
    const acc   = document.getElementById('comments-accordion');
    const empty = document.getElementById('comments-empty');
    acc.innerHTML = '';
    if (!posts.length) { empty.style.display = ''; return; }
    empty.style.display = 'none';

    for (const p of posts) {
        const id    = p.postID ?? p.PostID;
        const title = p.title  ?? p.Title ?? '(no title)';
        const votes = p.votes  ?? p.Votes ?? 0;

        const item = document.createElement('div');
        item.className = 'accordion-item mb-2';
        item.id = `comments-post-${id}`;
        item.dataset.postTitle = title.toLowerCase();
        item.innerHTML = `
          <h2 class="accordion-header">
            <button class="accordion-button collapsed" type="button"
              data-bs-toggle="collapse" data-bs-target="#collapse-post-${id}"
              aria-expanded="false">
              <span class="me-2 text-muted" style="font-size:0.8rem;min-width:40px">#${id}</span>
              <span class="truncate flex-grow-1" title="${esc(title)}">${esc(title)}</span>
              <span class="ms-3 badge ${votes >= 0 ? 'bg-success' : 'bg-danger'}">${votes >= 0 ? '+' : ''}${votes}</span>
            </button>
          </h2>
          <div id="collapse-post-${id}" class="accordion-collapse collapse" data-post-id="${id}">
            <div class="accordion-body p-0">
              <div class="comment-loading text-muted text-center py-3" style="font-size:0.85rem">
                Click to load comments…
              </div>
              <div class="comment-list"></div>
            </div>
          </div>`;

        const collapseEl = item.querySelector('.accordion-collapse');
        let loaded = false;
        collapseEl.addEventListener('show.bs.collapse', async () => {
            if (loaded) return;
            loaded = true;
            const loadingEl = item.querySelector('.comment-loading');
            loadingEl.textContent = '⏳ Loading comments…';
            try {
                const comments = await api.getPostComments(id);
                loadingEl.remove();
                renderCommentsInAccordion(item, id, comments);
            } catch {
                loadingEl.textContent = '⚠ Failed to load comments.';
                loaded = false;
            }
        });

        acc.appendChild(item);
    }
}

function renderCommentsInAccordion(item, postId, comments) {
    const list = item.querySelector('.comment-list');
    if (!comments || !comments.length) {
        list.innerHTML = `<p class="text-muted text-center py-3 mb-0" style="font-size:0.85rem">No comments on this post.</p>`;
        return;
    }
    const table = document.createElement('table');
    table.className = 'table table-hover mb-0';
    table.innerHTML = `
      <thead>
        <tr>
          <th style="width:70px;font-size:0.78rem">ID</th>
          <th style="font-size:0.78rem">Comment</th>
          <th style="width:120px;font-size:0.78rem">Author</th>
          <th style="width:120px;font-size:0.78rem">Date</th>
          <th style="width:90px;font-size:0.78rem">Actions</th>
        </tr>
      </thead>`;
    const tbody = document.createElement('tbody');

    for (const c of comments) {
        const cid     = c.commentID ?? c.CommentID;
        const content = c.commentcontent ?? c.Commentcontent ?? '–';
        const author  = c.user?.username ?? c.User?.Username ?? c.user?.UserName ?? '?';
        const date    = c.created_at ?? c.Created_at;
        const dateStr = date ? new Date(date).toLocaleDateString() : '–';

        const tr = document.createElement('tr');
        tr.innerHTML = `
          <td class="text-muted">${cid}</td>
          <td><span class="truncate d-inline-block" style="max-width:350px" title="${esc(content)}">${esc(content)}</span></td>
          <td>${esc(author)}</td>
          <td class="text-muted" style="font-size:0.82rem">${dateStr}</td>
          <td>
            <button class="btn btn-outline-danger btn-action">🗑 Delete</button>
          </td>`;

        tr.querySelector('button').addEventListener('click', () =>
            confirmDelete(`Delete comment #${cid} by "${author}"?`, async () => {
                try {
                    await api.deleteComment(cid);
                    tr.remove();
                    showToast('Comment deleted.');
                } catch (e) {
                    showToast('Failed to delete comment: ' + (e.body?.message ?? e.status), 'danger');
                }
            })
        );

        tbody.appendChild(tr);
    }
    table.appendChild(tbody);
    list.appendChild(table);
}

// ─── Categories tab ──────────────────────────────────────────────────────────

function renderCats(cats) {
    const tbody = document.getElementById('cats-tbody');
    tbody.innerHTML = '';
    if (!cats.length) {
        tbody.innerHTML = `<tr><td colspan="3" class="text-center text-muted py-3">No categories yet.</td></tr>`;
        return;
    }
    for (const c of cats) {
        const id   = c.categoryID ?? c.CategoryID;
        const name = c.categoryname ?? c.CategoryName ?? '?';
        const tr = document.createElement('tr');
        tr.dataset.catId = id;
        tr.innerHTML = `
          <td class="text-muted">${id}</td>
          <td>${esc(name)}</td>
          <td>
            <button class="btn btn-outline-danger btn-action">🗑 Delete</button>
          </td>`;

        tr.querySelector('button').addEventListener('click', () =>
            confirmDelete(`Delete category "${name}" (ID ${id})? All posts in this category may be affected.`, async () => {
                try {
                    await api.deleteCat(id);
                    allCats = allCats.filter(x => (x.categoryID ?? x.CategoryID) !== id);
                    tr.remove();
                    updateStats();
                    showToast(`Category "${name}" deleted.`);
                } catch (e) {
                    showToast('Failed to delete category: ' + (e.body?.message ?? e.status), 'danger');
                }
            })
        );
        tbody.appendChild(tr);
    }
}

// ─── Reports tab ─────────────────────────────────────────────────────────────

function isOpenReport(status) {
    return String(status ?? '').trim().toLowerCase() === 'open';
}

function renderReports(reports) {
    const tbody = document.getElementById('reports-tbody');
    tbody.innerHTML = '';

    if (!reports.length) {
        tbody.innerHTML = `<tr><td colspan="8" class="text-center text-muted py-3">No reports found.</td></tr>`;
        return;
    }

    for (const r of reports) {
        const reportId = r.reportID ?? r.ReportID;
        const postId = r.postID ?? r.PostID;
        const postTitle = r.postTitle ?? r.PostTitle ?? '(deleted post)';
        const reporter = r.reporterUsername ?? r.ReporterUsername ?? '?';
        const reporterUserId = r.reporterUserID ?? r.ReporterUserID ?? '?';
        const reason = r.reason ?? r.Reason ?? '';
        const status = r.reportStatus ?? r.ReportStatus ?? 'Open';
        const created = r.created_at ?? r.Created_at;
        const dateStr = created ? new Date(created).toLocaleDateString() : '–';
        const open = isOpenReport(status);

        const tr = document.createElement('tr');
        tr.dataset.reportId = reportId;
        tr.dataset.search = `${postTitle} ${reporter} ${reason}`.toLowerCase();
        tr.innerHTML = `
          <td class="text-muted">${reportId}</td>
          <td class="text-muted">${postId}</td>
          <td><span class="truncate d-inline-block" title="${esc(postTitle)}">${esc(postTitle)}</span></td>
          <td class="text-muted" title="User ID: ${reporterUserId}">${esc(reporter)}</td>
          <td><span class="truncate d-inline-block" style="max-width:300px" title="${esc(reason)}">${esc(reason)}</span></td>
          <td class="text-muted" style="font-size:0.85rem">${dateStr}</td>
          <td>
            <span class="badge ${open ? 'bg-warning text-dark' : 'bg-secondary'}">${esc(status)}</span>
          </td>
          <td>
            <div class="d-flex gap-1 flex-wrap">
              <button class="btn btn-outline-success btn-action btn-report-keep" ${open ? '' : 'disabled'}>✓ Keep</button>
              <button class="btn btn-outline-danger btn-action btn-report-delete" ${open ? '' : 'disabled'}>🗑 Delete Post</button>
            </div>
          </td>`;

        tr.querySelector('.btn-report-keep').addEventListener('click', async () => {
            if (!confirm(`Close report #${reportId} and keep post #${postId}?`)) return;
            try {
                await api.resolveReportKeep(reportId);
                updateReportStatusInRow(tr, reportId, 'Closed (not deleted)');
                showToast(`Report #${reportId} closed (post kept).`);
            } catch (e) {
                showToast('Failed to resolve report: ' + (e.body?.message ?? e.status), 'danger');
            }
        });

        tr.querySelector('.btn-report-delete').addEventListener('click', async () => {
            if (!confirm(`Delete post #${postId} and close report #${reportId}?`)) return;
            try {
                await api.resolveReportDelete(reportId);
                // Refresh reports and posts so any other report on this post is updated too.
                await Promise.all([loadReports(), loadPosts()]);
                updateStats();
                showToast(`Report #${reportId} closed and post deleted.`);
            } catch (e) {
                showToast('Failed to delete post from report: ' + (e.body?.message ?? e.status), 'danger');
            }
        });

        tbody.appendChild(tr);
    }
}

function updateReportStatusInRow(tr, reportId, newStatus) {
    const report = allReports.find(x => (x.reportID ?? x.ReportID) === reportId);
    if (report) {
        report.reportStatus = newStatus;
        report.ReportStatus = newStatus;
    }

    const badge = tr.querySelector('td:nth-child(7) .badge');
    if (badge) {
        badge.textContent = newStatus;
        badge.className = 'badge bg-secondary';
    }
    tr.querySelectorAll('button').forEach(btn => btn.disabled = true);
}

// ─── Data loading ────────────────────────────────────────────────────────────

async function loadUsers() {
    try {
        allUsers = await api.getAllUsers();
        renderUsers(allUsers);
    } catch {
        showToast('Could not load users.', 'danger');
    }
}

async function loadPosts() {
    try {
        allPosts = await api.getAllPosts();
        renderPosts(allPosts);
        buildCommentsAccordion(allPosts);
    } catch {
        showToast('Could not load posts.', 'danger');
    }
}

async function loadCats() {
    try {
        allCats = await api.getAllCats();
        renderCats(allCats);
    } catch {
        showToast('Could not load categories.', 'danger');
    }
}

async function loadReports() {
    try {
        allReports = await api.getAllReports();
        renderReports(allReports);
    } catch {
        showToast('Could not load reports.', 'danger');
    }
}

async function loadAll() {
    await Promise.all([loadUsers(), loadPosts(), loadCats(), loadReports()]);
    updateStats();
}

// ─── Search / filter ─────────────────────────────────────────────────────────

function filterTable(inputId, rowSelector, keyFn) {
    document.getElementById(inputId).addEventListener('input', function () {
        const q = this.value.trim().toLowerCase();
        document.querySelectorAll(rowSelector).forEach(row => {
            row.style.display = keyFn(row).includes(q) ? '' : 'none';
        });
    });
}

// ─── Utility ─────────────────────────────────────────────────────────────────

function esc(str) {
    return String(str ?? '')
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;');
}

// ─── Init ─────────────────────────────────────────────────────────────────────

async function init() {
    const loading = document.getElementById('admin-loading');
    const guard   = document.getElementById('admin-guard');
    const panel   = document.getElementById('admin-panel');

    // Bootstrap modal instances
    deleteModal = new bootstrap.Modal(document.getElementById('deleteModal'));
    renameModal = new bootstrap.Modal(document.getElementById('renameModal'));

    // Confirm-delete button
    document.getElementById('delete-confirm-btn').addEventListener('click', async () => {
        if (pendingDeleteFn) {
            deleteModal.hide();
            await pendingDeleteFn();
            pendingDeleteFn = null;
        }
    });

    // Rename confirm
    document.getElementById('rename-confirm-btn').addEventListener('click', async () => {
        const id      = parseInt(document.getElementById('rename-user-id').value, 10);
        const newName = document.getElementById('rename-input').value.trim();
        if (!newName) return;
        try {
            await api.renameUser(id, newName);
            // Update in-memory
            const u = allUsers.find(x => (x.userID ?? x.UserID) === id);
            if (u) { u.username = newName; u.Username = newName; }
            // Update table row
            const tr = document.querySelector(`#users-tbody tr[data-user-id="${id}"]`);
            if (tr) {
                tr.querySelector('strong').textContent = newName;
                tr.dataset.username = newName.toLowerCase();
            }
            renameModal.hide();
            showToast(`User renamed to "${newName}".`);
        } catch (e) {
            showToast('Failed to rename: ' + (e.body?.message ?? e.status), 'danger');
        }
    });

    // Category create form
    document.getElementById('cat-create-form').addEventListener('submit', async (e) => {
        e.preventDefault();
        const input = document.getElementById('cat-new-name');
        const name  = input.value.trim();
        if (!name) return;
        try {
            await api.createCat(name);
            // Reload categories
            allCats = await api.getAllCats();
            renderCats(allCats);
            updateStats();
            input.value = '';
            showToast(`Category "${name}" created.`);
        } catch (e) {
            showToast('Failed to create category: ' + (e.body?.message ?? e.status), 'danger');
        }
    });

    // Refresh button
    document.getElementById('admin-refresh-btn').addEventListener('click', async () => {
        showToast('Refreshing data…', 'info', 1500);
        await loadAll();
        updateStats();
        showToast('Data refreshed.');
    });

    // Search filters
    filterTable('user-search',        '#users-tbody tr',  row => `${row.dataset.username ?? ''} ${row.dataset.role ?? ''}`);
    filterTable('post-search',        '#posts-tbody tr',  row => row.dataset.postTitle ?? '');
    filterTable('comment-post-search','#comments-accordion .accordion-item', row => row.dataset.postTitle ?? '');
    filterTable('report-search',      '#reports-tbody tr', row => row.dataset.search ?? '');

    // ── Auth check
    let user;
    try {
        user = await getMe();
    } catch {
        window.location.href = '/login.html';
        return;
    }

    loading.style.display = 'none';

    const role = (user.Role ?? user.role ?? '').trim();
    if (role.toLowerCase() !== 'admin') {
        guard.style.display = '';
        return;
    }

    document.getElementById('admin-user-display').textContent  = user.UserName ?? user.username ?? '';
    document.getElementById('admin-navbar-user').textContent   = user.UserName ?? user.username ?? '';
    panel.style.display = '';

    await loadAll();
}

// Module scripts are deferred — DOM is already ready when this runs.
// Guard against the rare case where DOMContentLoaded already fired.
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', init);
} else {
    init();
}
