import { getPost, getPosts, searchPosts, vote, favouritePost, createReport } from './api.js';
import {
    applyLocalVoteToggle,
    applyVoteState,
    applyVoteStateToList,
    syncVoteStateForUser
} from './voteState.js';

const feed = document.getElementById('post-feed');
let currentCategory = null;
let currentSearch = null;
let currentVoteUserId = null;
let reportModal = null;

/** Fetch the logged-in user's vote history so buttons show filled/outlined state. */
export async function loadUserVotes(userId) {
    currentVoteUserId = Number.isFinite(Number(userId)) ? Number(userId) : null;
    await syncVoteStateForUser(currentVoteUserId);
    applyVoteStateToList(feed);
}

if (feed) initFeed();

export function filterBySearch(query) {
    currentSearch = query?.trim() || null;
    currentCategory = null;
    [...feed.querySelectorAll('.post-card')].forEach(el => el.remove());
    feed._resetFeed?.();

    // Update active state in sidebar (clear category highlights when searching)
    document.querySelectorAll('.sidebar-link[data-cat]').forEach(a => {
        a.classList.toggle('active', !currentSearch && (a.dataset.cat || null) === currentCategory);
    });
}

export function filterByCategory(category) {
    currentCategory = category || null;
    currentSearch = null;
    // Clear existing posts (but keep the sentinel)
    [...feed.querySelectorAll('.post-card')].forEach(el => el.remove());
    // Reset state and re-trigger the sentinel observer
    feed._resetFeed?.();

    // Update active state in sidebar
    // Normalise empty-string data-cat ("All") to null so it matches correctly
    document.querySelectorAll('.sidebar-link[data-cat]').forEach(a => {
        a.classList.toggle('active', (a.dataset.cat || null) === currentCategory);
    });
}

async function initFeed() {
    let page = 0;
    let loading = false;
    let allLoaded = false;

    // Sentinel div triggers loading more posts when visible
    const sentinel = document.createElement('div');
    sentinel.id = 'feed-sentinel';
    sentinel.style.height = '1px';
    feed.appendChild(sentinel);

    // Unload posts that are far off screen (replace with spacer)
    // States: loaded (no flags) | unloaded (data-unloaded='1') | reloading (data-unloaded='1' + _reloadController)
    const unloadObserver = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            const el = entry.target;
            if (!entry.isIntersecting) {
                // Went off-screen: abort any in-flight reload
                if (el._reloadController) {
                    el._reloadController.abort();
                    el._reloadController = null;
                }
                // If still fully loaded, unload it now
                if (!el.dataset.unloaded) {
                    el.dataset.height = el.offsetHeight;
                    el.dataset.unloaded = '1';
                    el.innerHTML = '';
                    el.style.height = el.dataset.height + 'px';
                }
            } else if (el.dataset.unloaded && !el._reloadController) {
                // Came on-screen and not already reloading
                reloadPost(el);
            }
        });
    }, { root: feed, rootMargin: '200px 0px' }); // unload only when 200px outside the viewport

    // Allow external reset (category switch)
    feed._resetFeed = () => {
        page = 0;
        loading = false;
        allLoaded = false;
        loadObserver.unobserve(sentinel);
        loadObserver.observe(sentinel);
    };

    // Sentinel observer loads the next page when near bottom
    const loadObserver = new IntersectionObserver(async (entries) => {
        if (!entries[0].isIntersecting || loading || allLoaded) return;
        loading = true;
        try {
            let posts;
            if (currentSearch) {
                posts = await searchPosts(currentSearch, currentCategory ?? 'All');
                allLoaded = true; // search returns all results at once
            } else {
                posts = await getPosts(page, 10, currentCategory);
                page++;
            }
            if (!posts || posts.length === 0) { allLoaded = true; return; }
            posts.forEach(post => renderPost(post, unloadObserver));
        } finally {
            loading = false;
            feed.insertBefore(sentinel, null); // keep sentinel at the end
        }
    }, { root: feed, rootMargin: '400px' }); // start loading 400px before hitting the bottom

    loadObserver.observe(sentinel);
}

function renderPost(post, unloadObserver) {
    const el = document.createElement('div');
    el.className = 'card mb-3 post-card';
    el.dataset.postId = post.postID;
    fillPostContent(el, post);

    const sentinel = document.getElementById('feed-sentinel');
    feed.insertBefore(el, sentinel);
    unloadObserver.observe(el);
}

function fillPostContent(el, post) {
    el.innerHTML = `
      <div class="card-body">
        <div class="d-flex justify-content-between align-items-start">
          <div style="min-width:0;flex:1">
            <span class="badge badge-cat mb-2">${escapeHtml(post.category?.categoryname ?? '')}</span>
            <h5 class="card-title mb-1">
              <a href="post.html?id=${post.postID}" class="post-title-link">
                ${escapeHtml(post.title)}
              </a>
            </h5>
            <small class="text-muted">
              by <span style="color:var(--accent);font-weight:600">${escapeHtml(post.user?.username ?? post.username ?? 'unknown')}</span>
              &nbsp;·&nbsp;${new Date(post.created_at).toLocaleDateString(undefined, {year:'numeric',month:'short',day:'numeric'})}
            </small>
          </div>
          <div class="d-flex flex-column align-items-center ms-3 vote-box">
            <button class="btn btn-sm btn-upvote">▲</button>
            <strong class="vote-count">${post.votes ?? 0}</strong>
            <button class="btn btn-sm btn-downvote">▼</button>
          </div>
        </div>
        <p class="card-text mt-2 mb-2 text-truncate" style="color:var(--muted);font-size:.9rem">${escapeHtml(post.content)}</p>
        <div class="d-flex gap-2 align-items-center mt-1 flex-wrap">
          <a href="post.html?id=${post.postID}" class="btn btn-sm btn-outline-secondary" style="border-radius:99px;font-size:.8rem">
            💬 ${post.comments?.length ?? 0} comments
          </a>
          <button class="btn btn-sm btn-fav" data-fav="false" style="border-radius:99px;font-size:.8rem;border:1px solid var(--border);background:transparent;color:var(--muted)">
            ♡ Save
          </button>
                    <button class="btn btn-sm btn-report" style="border-radius:99px;font-size:.8rem;border:1px solid #f39c12;background:transparent;color:#f39c12">
                        ⚑ Report
                    </button>
        </div>
      </div>`;

    el.querySelector('.btn-upvote').addEventListener('click', () => handleVote(el, post.postID, true));
    el.querySelector('.btn-downvote').addEventListener('click', () => handleVote(el, post.postID, false));
    el.querySelector('.btn-fav').addEventListener('click', (e) => handleFavourite(e.currentTarget, post.postID));
    el.querySelector('.btn-report').addEventListener('click', () => openReportModal(post.postID));
    applyVoteState(el, post);
}

function ensureReportModal() {
    if (reportModal) return reportModal;

    const modalRoot = document.createElement('div');
    modalRoot.className = 'modal fade';
    modalRoot.id = 'reportPostModal';
    modalRoot.tabIndex = -1;
    modalRoot.setAttribute('aria-hidden', 'true');
    modalRoot.innerHTML = `
      <div class="modal-dialog">
        <div class="modal-content" style="background:var(--card);color:var(--fg);border-color:var(--border)">
          <div class="modal-header" style="border-color:var(--border)">
            <h5 class="modal-title">Report Post</h5>
            <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal" aria-label="Close"></button>
          </div>
          <div class="modal-body">
            <input type="hidden" id="report-post-id" />
            <label for="report-reason" class="form-label">Reason</label>
            <textarea id="report-reason" class="form-control" rows="4" maxlength="500" placeholder="Describe why this post should be reviewed..."></textarea>
            <div id="report-error" class="text-danger mt-2" style="display:none"></div>
          </div>
          <div class="modal-footer" style="border-color:var(--border)">
            <button type="button" class="btn btn-outline-secondary" data-bs-dismiss="modal">Cancel</button>
            <button type="button" class="btn btn-warning" id="submit-report-btn">Submit report</button>
          </div>
        </div>
      </div>`;

    document.body.appendChild(modalRoot);
    const modal = new bootstrap.Modal(modalRoot);
    const submitBtn = modalRoot.querySelector('#submit-report-btn');
    const reasonEl = modalRoot.querySelector('#report-reason');
    const postIdEl = modalRoot.querySelector('#report-post-id');
    const errorEl = modalRoot.querySelector('#report-error');

    submitBtn.addEventListener('click', async () => {
        const postId = Number(postIdEl.value);
        const reason = reasonEl.value.trim();

        if (!reason) {
            errorEl.textContent = 'Please add a report reason.';
            errorEl.style.display = '';
            return;
        }

        submitBtn.disabled = true;
        try {
            await createReport(postId, reason);
            modal.hide();

            const reportBtn = document.querySelector(`.post-card[data-post-id="${postId}"] .btn-report`);
            if (reportBtn) {
                reportBtn.disabled = true;
                reportBtn.textContent = '⚑ Reported';
                reportBtn.style.opacity = '0.7';
            }

            alert('Report submitted. Thank you.');
        } catch (err) {
            if (err?.status === 401) {
                errorEl.textContent = 'You must be logged in to report posts.';
            } else if (err?.status === 409) {
                errorEl.textContent = err?.body?.message ?? err?.body?.title ?? 'You already have an open report for this post.';
            } else {
                errorEl.textContent = err?.body?.message ?? err?.body?.title ?? 'Could not submit report.';
            }
            errorEl.style.display = '';
        } finally {
            submitBtn.disabled = false;
        }
    });

    modalRoot.addEventListener('show.bs.modal', () => {
        reasonEl.value = '';
        errorEl.textContent = '';
        errorEl.style.display = 'none';
    });

    reportModal = modal;
    return reportModal;
}

function openReportModal(postId) {
    const modal = ensureReportModal();
    const modalRoot = document.getElementById('reportPostModal');
    modalRoot.querySelector('#report-post-id').value = String(postId);
    modal.show();
}

async function handleFavourite(btn, postId) {
    try {
        await favouritePost(postId);
        const saved = btn.dataset.fav !== 'true';
        btn.dataset.fav = saved ? 'true' : 'false';
        btn.textContent = saved ? '❤ Saved' : '♡ Save';
        btn.style.color = saved ? 'var(--accent)' : 'var(--muted)';
        btn.style.borderColor = saved ? 'var(--accent)' : 'var(--border)';
    } catch (err) {
        if (err?.status === 401) alert('You must be logged in to save posts.');
        else alert('Could not update favourites.');
    }
}

async function handleVote(el, postId, isUpvote) {
    try {
        await vote(postId, isUpvote);
        // Refresh the vote count from the server
        const updated = await getPost(postId);
        el.querySelector('.vote-count').textContent = updated.votes ?? 0;

        if (currentVoteUserId != null) {
            await syncVoteStateForUser(currentVoteUserId);
        } else {
            applyLocalVoteToggle(postId, isUpvote);
        }
        applyVoteState(el, postId);
    } catch (err) {
        if (err?.status === 401) {
            alert('You must be logged in to vote.');
        } else {
            alert('Could not register vote. Please try again.');
        }
    }
}

function reloadPost(el) {
    // Re-fetch and re-fill post when scrolled back into view.
    // data-unloaded stays set until content arrives so the observer
    // cannot re-unload the card or start a second fetch mid-flight.
    const id = el.dataset.postId;
    const controller = new AbortController();
    el._reloadController = controller;
    getPost(id)
        .then(post => {
            if (controller.signal.aborted) return;
            el._reloadController = null;
            delete el.dataset.unloaded;
            el.style.height = '';
            fillPostContent(el, post);
        })
        .catch(() => {
            if (controller.signal.aborted) return;
            el._reloadController = null;
            delete el.dataset.unloaded;
            el.style.height = '';
            el.innerHTML = '<div class="card-body text-muted">Failed to reload post.</div>';
        });
}

function escapeHtml(str) {
    return String(str ?? '').replace(/&/g,'&amp;').replace(/</g,'&lt;').replace(/>/g,'&gt;');
}   
