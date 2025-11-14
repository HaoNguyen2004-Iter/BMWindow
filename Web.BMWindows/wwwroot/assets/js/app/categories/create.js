import { toast } from '/assets/js/app/common/toast.js';

function onCreateCategoryClick(e) {
    const btn = e.target.closest('#btnCreateCategory') || e.target.closest('#btnSaveCategory');
    if (!btn) return;

    if (btn.id === 'btnSaveCategory') {
        const formCheck = document.getElementById('categoryForm');
        const fdCheck = formCheck ? new FormData(formCheck) : null;
        const idCheck = fdCheck ? parseInt(fdCheck.get('Id') || '0', 10) : 0;
        if (idCheck > 0) return; 
    } 

    const form = document.getElementById('categoryForm');
    if (!form) {
        console.error('Không tìm thấy form #categoryForm');
        toast?.error('Không tìm thấy form');
        return;
    }

    const fd = new FormData(form);
    const url = '/Category/Create';

    btn.disabled = true;
    const originalText = btn.textContent;
    btn.textContent = 'Đang xử lý...';

    fetch(url, { method: 'POST', body: fd })
        .then(async resp => {
            let json;
            try {
                json = await resp.json();
            } catch {
                json = { ok: false, message: 'Phản hồi không hợp lệ' };
            }

            if (!resp.ok || !json.ok) {
                toast?.error(json.message || 'Thao tác thất bại');
                return;
            }

            toast?.success(json.message || 'Tạo thành công');

            // Reload list
            const host = document.getElementById('category-management');
            if (host) {
                if (window.$) $('#category-management').removeData('loaded');
                if (typeof window.loadCategory === 'function') window.loadCategory();
            }

            // Close modal
            const modalEl = document.getElementById('global-modal');
            if (modalEl) bootstrap.Modal.getInstance(modalEl)?.hide();
        })
        .catch(ex => {
            console.error(ex);
            toast?.error('Lỗi kết nối');
        })
        .finally(() => {
            btn.disabled = false;
            btn.textContent = originalText;
        });
}

document.addEventListener('click', onCreateCategoryClick);