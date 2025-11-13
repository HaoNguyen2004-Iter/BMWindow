// Placeholder for appitems upload behaviors
// Implement file upload helpers and progress UI here

export async function uploadAppItemFiles(formElement) {
    const fd = new FormData(formElement);
    const resp = await fetch('/AppItem/Upload', { method: 'POST', body: fd });
    if (!resp.ok) throw new Error(`HTTP ${resp.status}`);
    return resp.json();
}

if (typeof window !== 'undefined') window.uploadAppItemFiles = uploadAppItemFiles;
