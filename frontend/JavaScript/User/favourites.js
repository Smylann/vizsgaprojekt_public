import { getMe, logout, getFavourites, favouritePost } from './api.js';

function escapeHtml(str) {
    return String(str ?? '').replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;');
}

function renderCard(post) {
    const id      = post.postID  ?? post.PostID;
    const title   = post.title   ?? post.Title   ?? '(no title)';
    const content = post.content ?? post.Content ?? '';
    const votes   = post.votes   ?? post.Votes   ?? 0;
    const created = post.created_at ?? post.Created_at;
    const dateStr = created
        ? new Date(created).toLocaleDateString(undefined, { year: 'numeric', month: 'short', day: 'numeric' })
        : '';

    const el = document.createElement('div');
    el.className = 'card mb-3 post-card';
    el.dataset.postId = id;
    el.innerHTML = `
      <div class="card-body">
        <div class="d-flex justify-content-between align-items-start">
          <div style="min-width:0;flex:1">
            <h5 class="card-title mb-1">
              <a href="post.html?id=${id}" class="post-title-link">${escapeHtml(title)}</a>
            </h5>
            <small class="text-muted">${dateStr}</small>
          </div>
          <div class="d-flex flex-column align-items-center ms-3">
            <span class="${votes >= 0 ? 'text-success' : 'text-danger'} fw-semibold" style="font-size:1.1rem">${votes >= 0 ? '+' : ''}${votes}</span>
            <small class="text-muted" style="font-size:0.7rem">votes</small>
          </div>
        </div>
        <p class="card-text mt-2 mb-2 text-truncate" style="color:var(--muted);font-size:.9rem">${escapeHtml(content)}</p>
        <div class="d-flex gap-2 flex-wrap">
          <a href="post.html?id=${id}" class="btn btn-sm btn-outline-secondary" style="border-radius:99px;font-size:.8rem">
            →  Open Post
          </a>
          <button class="btn btn-sm btn-unsave" style="border-radius:99px;font-size:.8rem;border:1px solid var(--accent);background:transparent;color:var(--accent)">
            ✕ Remove
          </button>
        </div>
      </div>`;

    el.querySelector('.btn-unsave').addEventListener('click', async () => {
        try {
            await favouritePost(id);
            el.style.transition = 'opacity 0.3s';
            el.style.opacity = '0';
            setTimeout(() => {
                el.remove();
                updateCount();
            }, 300);
        } catch (e) {
            alert('Could not remove from favourites.');
        }
    });

    return el;
}

function updateCount() {
    const remaining = document.querySelectorAll('#fav-list .post-card').length;
    const badge = document.getElementById('fav-count');
    badge.textContent = remaining;
    if (remaining === 0) {
        document.getElementById('fav-empty').style.display = '';
    }
}

async function init() {
    // Auth check
    let user;
    try {
        user = await getMe();
    } catch {
        window.location.href = '/login.html';
        return;
    }

    // Populate navbar username
    const navUser = document.getElementById('navbar-username');
    if (navUser) {
        navUser.textContent    = user.UserName ?? user.username ?? '';
        navUser.style.display  = '';
    }

    // Logout button
    const logoutBtn = document.getElementById('logout-btn');
    if (logoutBtn) {
        logoutBtn.style.display = '';
        logoutBtn.addEventListener('click', async () => {
            await logout();
            window.location.href = '/login.html';
        });
    }

    const userId    = user.ID ?? user.id;
    const loadingEl = document.getElementById('fav-loading');
    const emptyEl   = document.getElementById('fav-empty');
    const errorEl   = document.getElementById('fav-error');
    const listEl    = document.getElementById('fav-list');
    const badge     = document.getElementById('fav-count');

    try {
        const favourites = await getFavourites(userId);

        loadingEl.style.display = 'none';

        if (!favourites || favourites.length === 0) {
            emptyEl.style.display = '';
            return;
        }

        badge.textContent   = favourites.length;
        badge.style.display = '';

        for (const post of favourites) {
            listEl.appendChild(renderCard(post));
        }
    } catch {
        loadingEl.style.display = 'none';
        errorEl.style.display   = '';
    }
}

if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', init);
} else {
    init();
}
