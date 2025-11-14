// Category list loader with pagination support
import Pagination from '../common/pagination.js';

let categoryPagination = null;

function loadCategory() {
    const $container = $('#category-management');
    if ($container.length === 0) {
        console.error('Không tìm thấy container #category-management để nạp partial.');
        return;
    }
    if ($container.data('loaded')) {
        // Đã load rồi, chỉ reload data
        if (categoryPagination) {
            categoryPagination.reload();
        }
        return;
    }

    $container.html(
        '<div class="p-3 text-center text-muted">' +
        '<i class="fa fa-spinner fa-spin me-2"></i>Đang tải dữ liệu nhóm ứng dụng...' +
        '</div>'
    );

    $.ajax({
        url: '/Category/AppItemGroup',
        method: 'GET',
        cache: false
    })
    .done(function (html) {
        $container.html(html);
        $container.data('loaded', true);
        
        // Khởi tạo pagination sau khi load HTML xong
        setTimeout(() => {
            initCategoryPagination();
        }, 100);
    })
    .fail(function (xhr) {
        $container.html(
            '<div class="alert alert-danger m-3">' +
            'Lỗi tải dữ liệu nhóm ứng dụng (HTTP ' + xhr.status + ').' +
            '</div>'
        );
    });
}

// Khởi tạo pagination
function initCategoryPagination() {
    const container = document.querySelector('#category-management');
    if (!container) return;
    
    categoryPagination = new Pagination({
        apiUrl: '/Category/GetList',
        container: container,
        tableSelector: '.data-table tbody',
        paginationSelector: '.pagination-controls',
        limit: 5,
        orderBy: 'Prioritize',
        orderType: 'asc',
        onDataLoaded: renderCategoryTable,
        onError: (error) => {
            console.error('Lỗi load category:', error);
        }
    });
    
    // Load dữ liệu
    categoryPagination.loadData();
}

// Render table
function renderCategoryTable(categories) {
    const tbody = document.querySelector('#category-management .data-table tbody');
    if (!tbody) return;
    
    if (categories.length === 0) {
        tbody.innerHTML = '<tr><td colspan="8" class="text-center text-muted py-4">Không có dữ liệu nhóm ứng dụng</td></tr>';
        return;
    }
    
    let html = '';
    categories.forEach((item, index) => {
        const createTime = formatDate(item.createTime);
        const updateTime = formatDate(item.updateTime);
        
        html += `
            <tr>
                <td>${item.id}</td>
                <td><strong>${escapeHtml(item.name)}</strong></td>
                <td>${item.prioritize}</td>
                <td>${item.createBy}</td>
                <td>${createTime}</td>
                <td>${item.updateBy || '-'}</td>
                <td>${updateTime}</td>
                <td class="text-end">
                    <div class="btn-group">
                        <button class="btn btn-sm btn-warning modal-trigger"
                                data-modal-url="/Category/Detail"
                                data-modal-id="${item.id}"
                                data-modal-title="Chỉnh sửa Nhóm ứng dụng"
                                data-modal-size="md"
                                title="Chỉnh sửa">
                            <i class="fa-solid fa-pen"></i>
                        </button>
                        <button class="btn btn-sm btn-danger" onclick="deleteCategory(${item.id})" title="Xóa">
                            <i class="fa-solid fa-trash"></i>
                        </button>
                    </div>
                </td>
            </tr>
        `;
    });
    
    tbody.innerHTML = html;
}

function formatDate(dateString) {
    if (!dateString) return '-';
    const date = new Date(dateString);
    const day = String(date.getDate()).padStart(2, '0');
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const year = date.getFullYear();
    return `${day}/${month}/${year}`;
}

function escapeHtml(text) {
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}

// Expose for legacy usage
window.loadCategory = loadCategory;
window.deleteCategory = function(id) {
    if (confirm('Bạn có chắc muốn xóa nhóm ứng dụng này?')) {
        console.log('Delete category:', id);
        // TODO: Implement delete API
    }
};

export default loadCategory;
