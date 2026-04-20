import { getDislikedPosts, getLikedPosts } from './api.js';

let likedPostIds = new Set();
let dislikedPostIds = new Set();

export function resetVoteState() {
    likedPostIds = new Set();
    dislikedPostIds = new Set();
}

export function normalizePostId(postOrId) {
    if (typeof postOrId === 'number') return postOrId;
    if (!postOrId) return null;
    const raw = postOrId.postID ?? postOrId.PostID ?? postOrId.id ?? postOrId.ID ?? null;
    if (raw == null) return null;
    const parsed = Number(raw);
    return Number.isFinite(parsed) ? parsed : null;
}

export function getUserIdFromUser(user) {
    if (!user) return null;
    const raw = user.ID ?? user.userID ?? user.UserID ?? user.userId ?? user.UserId ?? user.id;
    if (raw == null) return null;
    const parsed = Number(raw);
    return Number.isFinite(parsed) ? parsed : null;
}

export async function syncVoteStateForUser(userId) {
    if (!Number.isFinite(userId)) {
        resetVoteState();
        return;
    }

    const [liked, disliked] = await Promise.allSettled([
        getLikedPosts(userId),
        getDislikedPosts(userId)
    ]);

    likedPostIds = liked.status === 'fulfilled'
        ? new Set((liked.value ?? []).map(normalizePostId).filter(id => id != null))
        : new Set();

    dislikedPostIds = disliked.status === 'fulfilled'
        ? new Set((disliked.value ?? []).map(normalizePostId).filter(id => id != null))
        : new Set();
}

export function applyVoteState(element, postOrId) {
    const postId = normalizePostId(postOrId);
    const upBtn = element?.querySelector?.('.btn-upvote');
    const downBtn = element?.querySelector?.('.btn-downvote');
    if (!upBtn || !downBtn || postId == null) return;

    const isLiked = likedPostIds.has(postId);
    const isDisliked = dislikedPostIds.has(postId);

    upBtn.classList.toggle('voted-up', isLiked);
    downBtn.classList.toggle('voted-down', isDisliked);
}

export function applyVoteStateToList(root = document) {
    root?.querySelectorAll?.('[data-post-id]').forEach((el) => {
        applyVoteState(el, Number(el.dataset.postId));
    });
}

export function applyVoteStateBySelector(postId, selectors) {
    const upBtn = document.querySelector(selectors.up);
    const downBtn = document.querySelector(selectors.down);
    if (!upBtn || !downBtn || !Number.isFinite(postId)) return;

    const isLiked = likedPostIds.has(postId);
    const isDisliked = dislikedPostIds.has(postId);

    upBtn.classList.toggle('voted-up', isLiked);
    downBtn.classList.toggle('voted-down', isDisliked);
}

export function applyLocalVoteToggle(postId, isUpvote) {
    if (!Number.isFinite(postId)) return;

    if (isUpvote) {
        if (likedPostIds.has(postId)) likedPostIds.delete(postId);
        else {
            likedPostIds.add(postId);
            dislikedPostIds.delete(postId);
        }
    } else {
        if (dislikedPostIds.has(postId)) dislikedPostIds.delete(postId);
        else {
            dislikedPostIds.add(postId);
            likedPostIds.delete(postId);
        }
    }
}