const Ajax = {
    get: async (url) => {
        const resp = await fetch(url, { headers: { 'X-Requested-With': 'XMLHttpRequest' } });
        if (!resp.ok) throw new Error(`HTTP ${resp.status}`);
        return resp.text();
    },
    getJson: async (url) => {
        const resp = await fetch(url, { headers: { 'X-Requested-With': 'XMLHttpRequest' } });
        if (!resp.ok) throw new Error(`HTTP ${resp.status}`);
        return resp.json();
    },
    postForm: async (url, formData) => {
        const resp = await fetch(url, { method: 'POST', body: formData });
        if (!resp.ok) throw new Error(`HTTP ${resp.status}`);
        return resp.json();
    }
};

// Expose to window for legacy modules
window.AppAjax = Ajax;

export default Ajax;
