import { toast } from '/assets/js/app/common/toast.js';

function onSaveCategoryClick(e) {
    const btn = e.target.closest('#btnSaveCategory');
    if (!btn) return;

    const form = document.getElementById('categoryForm');
    if (!form) {
        console.error('Không tìm thấy form #categoryForm');
        toast?.error('Không tìm thấy form');
        return;
    }

    const fd = new FormData(form);
    const id = parseInt(fd.get('Id') || '0', 10);
    const url = id > 0 ? '/Category/Update' : '/Category/Create';

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

            toast?.success(json.message || 'Thành công');

            const host = document.getElementById('category-management');
            if (host) {
                if (window.$) $('#category-management').removeData('loaded');
                if (typeof window.loadCategory === 'function') window.loadCategory();
            }

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

document.addEventListener('click', onSaveCategoryClick);

function initCategoryEdit() { }
if (typeof window !== 'undefined') window.initCategoryEdit = initCategoryEdit;

document.addEventListener('app:modal:loaded', () => {
    initCategoryEdit();
});