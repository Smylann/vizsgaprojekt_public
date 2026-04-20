const API_BASE = (window.__API_BASE_URL__ ?? '/api').replace(/\/$/, '');

//const API_BASE = "fcsab.ddns.net:3070/api";

async function apiFetch(path, options = {}) {
    const res = await fetch(`${API_BASE}${path}`, {
        headers: { 'Content-Type': 'application/json', ...options.headers },
        credentials: 'include', // always send the auth cookie
        ...options,
    });
    if (!res.ok) throw { status: res.status, body: await res.json().catch(() => null) };
    return res.json().catch(() => null);
}

// Auth
export const getMe = () => apiFetch('/User/me');
export async function login(username, password) {
    const res = await fetch(`${API_BASE}/User/login`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        credentials: 'include',
        body: JSON.stringify({
            username,
            password: password
        })
    });

    if (!res.ok) {
        const text = await res.text().catch(() => '');
        let err = { status: res.status, message: text || 'Login failed' };
        try {
            const json = text ? JSON.parse(text) : null;
            err.message = json?.message || json?.error || json?.title || err.message;
        } catch {}
        throw err;
    }

    return res.json().catch(() => ({}));
}
export const logout = () => apiFetch('/User/logout', { method: 'POST' });
export const register = ({ username, email, password }) => {
    return apiFetch('/User/registration', {
        method: 'POST',
        body: JSON.stringify({ username, email, password })
    });
};

// Posts (feed)
export const getPosts = (page = 0, pageSize = 10, category = null) =>
    apiFetch(`/News?page=${page}&pageSize=${pageSize}${category ? `&category=${encodeURIComponent(category)}` : ''}`);
export const getCategories = () => apiFetch('/News/getallcats');
export const getPost = (id) => apiFetch(`/News/${id}`);
export const createPost = (data) =>
    apiFetch('/News/create_posts', { method: 'POST', body: JSON.stringify(data) });

// Votes
export const vote = (postId, isUpvote) =>
    apiFetch('/News/vote', { method: 'POST', body: JSON.stringify({ postId, isUpvote }) });
export const getLikedPosts = (userId) => apiFetch(`/News/getlikedposts?id=${userId}`);
export const getDislikedPosts = (userId) => apiFetch(`/News/getdislikedposts?id=${userId}`);

// Search
export const searchPosts = (title, cat = 'All') =>
    apiFetch(`/News/search_post?title=${encodeURIComponent(title)}&cat=${encodeURIComponent(cat)}`);

// Comments
export const getComments = (postId) => apiFetch(`/News/${postId}/comments`);
export const addComment = (postId, content) =>
    apiFetch('/News/comment', { method: 'POST', body: JSON.stringify({ postID: postId, commentcontent: content }) });

// User settings
export const modifyPassword = (currentPassword, newPassword) =>
    apiFetch('/User/modifypassword', { method: 'PUT', body: JSON.stringify({ currentPassword, newPassword }) });

// Favourites
export const favouritePost = (postId) =>
    apiFetch('/News/favourite_posts', { method: 'POST', body: JSON.stringify({ postId }) });
export const getFavourites = (userId) => apiFetch(`/News/getfavorites?id=${userId}`);

// Reports
export const createReport = (postID, reportreason) =>
    apiFetch('/News/create_report', { method: 'POST', body: JSON.stringify({ postID, reportreason }) });
