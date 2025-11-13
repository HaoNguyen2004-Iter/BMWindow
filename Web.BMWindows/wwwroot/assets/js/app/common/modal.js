console.debug('[modal.js] initializer');
document.addEventListener('click', async (e) => {
    const trigger = e.target.closest('.modal-trigger');
    if (!trigger) return;
    console.debug('[modal.js] click detected on .modal-trigger', trigger);

    e.preventDefault();

    const url = trigger.getAttribute('data-modal-url');
    if (!url) {
        console.warn('[modal.js] modal-trigger missing data-modal-url', trigger);
        return;
    }

    const id = trigger.getAttribute('data-modal-id');
    const size = trigger.getAttribute('data-modal-size');
    const explicitTitle = trigger.getAttribute('data-modal-title') || '';

    const fullUrl = id ? `${url}?id=${encodeURIComponent(id)}` : url;

    const modalEl = document.getElementById('global-modal');
    const dialogEl = document.getElementById('global-modal-dialog');
    const titleEl = document.getElementById('global-modal-title');
    const bodyEl = document.getElementById('global-modal-body');
    const footerEl = document.getElementById('global-modal-footer');

    if (!modalEl || !dialogEl || !titleEl || !bodyEl || !footerEl) {
        console.error('Thiếu global modal host (#global-modal...)');
        return;
    }

    dialogEl.classList.remove('modal-sm', 'modal-lg', 'modal-xl');
    if (size && ['sm', 'lg', 'xl'].includes(size)) {
        dialogEl.classList.add(`modal-${size}`);
    }

    // Loading placeholder
    titleEl.textContent = explicitTitle || 'Đang tải...';
    bodyEl.innerHTML = '<div class="py-5 text-center text-muted"><i class="fa fa-spinner fa-spin me-2"></i>Đang tải...</div>';
    footerEl.classList.add('d-none');
    footerEl.innerHTML = '';

    try {
        const resp = await fetch(fullUrl, { headers: { 'X-Requested-With': 'XMLHttpRequest' } });
        const html = await resp.text();
        if (!resp.ok) {
            bodyEl.innerHTML = '<div class="alert alert-danger">Lỗi tải nội dung.</div>';
        } else {
            const temp = document.createElement('div');
            temp.innerHTML = html;
            const contentRoot = temp.querySelector('.modal-content') || temp;

            // Title
            let titleText = explicitTitle;
            const titleFromPartial = contentRoot.querySelector('.modal-title');
            if (!titleText && titleFromPartial) {
                titleText = titleFromPartial.textContent.trim();
            }
            titleEl.textContent = titleText;

            // Body
            const bodyFromPartial = contentRoot.querySelector('.modal-body');
            bodyEl.innerHTML = bodyFromPartial ? bodyFromPartial.innerHTML : contentRoot.innerHTML;

            // Footer
            const footerFromPartial = contentRoot.querySelector('.modal-footer');
            if (footerFromPartial) {
                footerEl.innerHTML = footerFromPartial.innerHTML;
                footerEl.classList.remove('d-none');
            } else {
                footerEl.classList.add('d-none');
                footerEl.innerHTML = '';
            }
        }

        // Notify modules that modal content is ready
        document.dispatchEvent(new CustomEvent('app:modal:loaded', {
            detail: { modalEl, dialogEl, titleEl, bodyEl, footerEl, trigger, url: fullUrl }
        }));

        bootstrap.Modal.getOrCreateInstance(modalEl).show();
    } catch (err) {
        console.error(err);
        bodyEl.innerHTML = '<div class="alert alert-danger">Lỗi kết nối.</div>';
    }
});

document.addEventListener('hidden.bs.modal', (e) => {
    if (e.target.id === 'global-modal') {
        document.getElementById('global-modal-body').innerHTML = '';
        document.getElementById('global-modal-footer').innerHTML = '';
        document.dispatchEvent(new CustomEvent('app:modal:closed', { detail: { modalEl: e.target } }));
    }
});