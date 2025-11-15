import Pagination from '../common/pagination.js';

let categoryPagination = null;
let switchedToFragment = false;

function loadCategory() {
    const container = document.querySelector('#category-management');
    if (!container) {
        console.error('Không tìm thấy container #category-management');
        return;
    }

    if (!categoryPagination) {
        categoryPagination = new Pagination({
            apiUrl: '/Category/AppItemGroup',       
            container: container,
            limit: 5,
            orderBy: 'Prioritize',
            orderType: 'asc',
            onAfterLoad: () => {
                if (!switchedToFragment) {
                    switchedToFragment = true;
                    categoryPagination.config.apiUrl = '/Category/AppItemGroup?part=fragment';
                    categoryPagination.config.replaceSelector = '#category-fragment';
                }
            }
        });
    }

    categoryPagination.reload();
}

window.loadCategory = loadCategory;
export default loadCategory;