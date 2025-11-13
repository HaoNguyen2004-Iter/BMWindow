// === TOAST ĐẸP HƠN - DÙNG BOOTSTRAP 5 ===
const TOAST_CONTAINER_ID = 'app-toast-container';

function ensureContainer(position = 'top-0 end-0') {
    let container = document.getElementById(TOAST_CONTAINER_ID);
    if (!container) {
        container = document.createElement('div');
        container.id = TOAST_CONTAINER_ID;
        container.className = `toast-container position-fixed p-3 ${position}`;
        container.style.zIndex = '1081';
        container.style.maxWidth = '380px';
        container.style.pointerEvents = 'none'; 
        document.body.appendChild(container);
    } else {
        container.className = container.className.replace(/top-\d|bottom-\d|start-\d|end-\d/g, '');
        position.split(' ').forEach(cls => container.classList.add(cls));
    }
    return container;
}

function createToast(message, type = 'success', delay = 3000, options = {}) {
    const { position = 'top-0 end-0', icon = true } = options;
    const container = ensureContainer(position);

    
    const icons = {
        success: '<i class="bi bi-check-circle-fill"></i>',
        error: '<i class="bi bi-x-circle-fill"></i>',
        danger: '<i class="bi bi-x-circle-fill"></i>',
        warning: '<i class="bi bi-exclamation-triangle-fill"></i>',
        warn: '<i class="bi bi-exclamation-triangle-fill"></i>',
        info: '<i class="bi bi-info-circle-fill"></i>'
    };

    const iconHtml = icon ? `<span class="me-2">${icons[type] || icons.success}</span>` : '';

    const styles = {
        success: 'bg-white border-start border-success border-4 text-success shadow',
        error: 'bg-white border-start border-danger border-4 text-danger shadow',
        danger: 'bg-white border-start border-danger border-4 text-danger shadow',
        warning: 'bg-white border-start border-warning border-4 text-warning shadow',
        warn: 'bg-white border-start border-warning border-4 text-warning shadow',
        info: 'bg-white border-start border-primary border-4 text-primary shadow'
    };

    const toastEl = document.createElement('div');
    toastEl.className = `toast align-items-center ${styles[type] || styles.success} rounded`;
    toastEl.setAttribute('role', 'alert');
    toastEl.setAttribute('aria-live', 'assertive');
    toastEl.setAttribute('aria-atomic', 'true');
    toastEl.style.pointerEvents = 'auto'; 
    toastEl.style.minWidth = '280px';
    toastEl.style.fontSize = '0.95rem';

    toastEl.innerHTML = `
        <div class="d-flex align-items-center p-1">
            <div class="toast-body d-flex align-items-center flex-grow-1">
                ${iconHtml}
                <span class="flex-grow-1">${message}</span>
            </div>
            <button type="button" class="btn-close btn-close-sm me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
        </div>
    `;

    container.appendChild(toastEl);

    const bsToast = bootstrap.Toast.getOrCreateInstance(toastEl, {
        delay,
        autohide: true
    });

    toastEl.addEventListener('hidden.bs.toast', () => toastEl.remove());

    setTimeout(() => toastEl.classList.add('show'), 10);

    bsToast.show();
    return bsToast;
}

export const toast = {
    success: (msg, delay = 3000, opts = {}) => createToast(msg, 'success', delay, { ...opts, icon: true }),
    error: (msg, delay = 4000, opts = {}) => createToast(msg, 'error', delay, { ...opts, icon: true }),
    info: (msg, delay = 3000, opts = {}) => createToast(msg, 'info', delay, { ...opts, icon: true }),
    warn: (msg, delay = 3500, opts = {}) => createToast(msg, 'warning', delay, { ...opts, icon: true })
};

export function showToast(message, options = {}) {
    const { type = 'success', delay = 3000, position } = options;
    return createToast(message, type, delay, { position });
}

if (typeof window !== 'undefined') {
    window.toast = toast;
    window.showToast = showToast;
}