/**
 * Pagination.js - Gọi API backend đã xử lý filter/paging
 * Backend: CategoryMany.GetAllCategory(CategoryModel, OptionResult)
 * Truyền filter (CategoryModel) + option (page/limit) lên server
 */

class Pagination {
    constructor(options = {}) {
        this.config = {
            apiUrl: options.apiUrl || '', // API endpoint
            container: options.container || null,
            tableSelector: options.tableSelector || 'tbody',
            paginationSelector: options.paginationSelector || '.pagination-controls',
            page: options.page || 1,
            limit: options.limit || 20,
            unlimited: options.unlimited || false,
            hasCount: options.hasCount !== false,
            orderBy: options.orderBy || '',
            orderType: options.orderType || 'asc',
            onDataLoaded: options.onDataLoaded || null,
            onError: options.onError || null
        };

        this.state = {
            currentPage: this.config.page,
            totalRecords: 0,
            totalPages: 0,
            loading: false
        };

        this.filters = {}; // CategoryModel filters
        this.init();
    }

    /**
     * Khởi tạo pagination
     */
    init() {
        if (this.config.container) {
            this.bindFilterEvents();
        }
    }

    /**
     * Gọi API backend với filter + option
     * Backend: CategoryController.GetList(CategoryModel + OptionResult)
     * Trả về: { ok, count, many, skip, take }
     */
    async loadData() {
        if (this.state.loading) return;
        
        this.state.loading = true;
        this.showLoading();

        try {
            const params = this.buildQueryParams();
            const url = `${this.config.apiUrl}?${params.toString()}`;
            
            const response = await fetch(url, {
                method: 'GET',
                headers: {
                    'X-Requested-With': 'XMLHttpRequest'
                }
            });

            if (!response.ok) {
                throw new Error(`HTTP ${response.status}`);
            }

            const result = await response.json();
            
            if (!result.ok) {
                throw new Error(result.message || 'Có lỗi xảy ra');
            }
            
            // Backend trả về: { ok, count, many, skip, take }
            this.state.totalRecords = result.count || 0;
            this.state.totalPages = this.config.unlimited ? 1 : Math.ceil(this.state.totalRecords / this.config.limit);
            
            this.hideLoading();
            this.renderPagination();

            if (typeof this.config.onDataLoaded === 'function') {
                this.config.onDataLoaded(result.many || [], result);
            }

        } catch (error) {
            this.hideLoading();
            console.error('Lỗi tải dữ liệu:', error);
            
            if (typeof this.config.onError === 'function') {
                this.config.onError(error);
            } else {
                this.showError(error.message || 'Không thể tải dữ liệu. Vui lòng thử lại.');
            }
        } finally {
            this.state.loading = false;
        }
    }

    /**
     * Build query params từ filters + option
     * Mapping với CategoryModel và OptionResult
     */
    buildQueryParams() {
        const params = new URLSearchParams();
        
        // CategoryModel filters
        if (this.filters.id) params.append('id', this.filters.id);
        if (this.filters.keyword) params.append('keyword', this.filters.keyword);
        if (this.filters.createDateFrom) params.append('createDateFrom', this.filters.createDateFrom);
        if (this.filters.createDateTo) params.append('createDateTo', this.filters.createDateTo);
        if (this.filters.updateDateFrom) params.append('updateDateFrom', this.filters.updateDateFrom);
        if (this.filters.updateDateTo) params.append('updateDateTo', this.filters.updateDateTo);

        // OptionResult params
        if (!this.config.unlimited) {
            params.append('page', this.state.currentPage);
            params.append('limit', this.config.limit);
        } else {
            params.append('unlimited', 'true');
        }
        
        params.append('hasCount', this.config.hasCount ? 'true' : 'false');
        
        if (this.config.orderBy) params.append('orderBy', this.config.orderBy);
        if (this.config.orderType) params.append('orderType', this.config.orderType);

        return params;
    }

    /**
     * Chuyển trang
     */
    goToPage(page) {
        const pageNum = parseInt(page);
        
        if (pageNum < 1 || pageNum > this.state.totalPages || pageNum === this.state.currentPage) {
            return;
        }

        this.state.currentPage = pageNum;
        this.loadData();
    }

    /**
     * Trang trước
     */
    previousPage() {
        if (this.state.currentPage > 1) {
            this.goToPage(this.state.currentPage - 1);
        }
    }

    /**
     * Trang sau
     */
    nextPage() {
        if (this.state.currentPage < this.state.totalPages) {
            this.goToPage(this.state.currentPage + 1);
        }
    }

    /**
     * Set filter value và reload
     */
    setFilter(key, value) {
        if (value === null || value === undefined || value === '') {
            delete this.filters[key];
        } else {
            this.filters[key] = value;
        }
        
        this.state.currentPage = 1; // Reset về trang 1
        this.loadData();
    }

    /**
     * Set multiple filters
     */
    setFilters(filters) {
        this.filters = { ...filters };
        this.state.currentPage = 1;
        this.loadData();
    }

    /**
     * Clear tất cả filters
     */
    clearFilters() {
        this.filters = {};
        this.state.currentPage = 1;
        
        // Clear input fields
        if (this.config.container) {
            this.config.container.querySelectorAll('.filter-row input, .filter-row select').forEach(input => {
                input.value = '';
            });
        }
        
        this.loadData();
    }

    /**
     * Bind events cho filter inputs
     */
    bindFilterEvents() {
        if (!this.config.container) return;

        const filterInputs = this.config.container.querySelectorAll('.filter-row input, .filter-row select');
        
        filterInputs.forEach(input => {
            const filterKey = input.dataset.filter || input.name;
            if (!filterKey) return;

            // Debounce cho text input
            let debounceTimer;
            const handleChange = (e) => {
                clearTimeout(debounceTimer);
                debounceTimer = setTimeout(() => {
                    this.setFilter(filterKey, e.target.value);
                }, 400);
            };

            if (input.tagName === 'SELECT') {
                input.addEventListener('change', (e) => {
                    this.setFilter(filterKey, e.target.value);
                });
            } else if (input.type === 'date') {
                input.addEventListener('change', (e) => {
                    this.setFilter(filterKey, e.target.value);
                });
            } else {
                input.addEventListener('input', handleChange);
            }
        });
    }

    /**
     * Render pagination controls
     */
    renderPagination() {
        const container = this.config.container?.querySelector(this.config.paginationSelector);
        if (!container) return;

        const { currentPage, totalPages, totalRecords } = this.state;
        
        if (totalRecords === 0) {
            container.innerHTML = '<div class="text-muted text-center py-3">Không có dữ liệu</div>';
            return;
        }

        const start = (currentPage - 1) * this.config.limit + 1;
        const end = Math.min(currentPage * this.config.limit, totalRecords);

        let html = `
            <div class="d-flex justify-content-between align-items-center">
                <div class="pagination-info text-muted small">
                    Hiển thị <strong>${start}</strong> - <strong>${end}</strong> trong tổng số <strong>${totalRecords}</strong> bản ghi
                </div>
                <nav>
                    <ul class="pagination pagination-sm mb-0">
        `;

        // Previous button
        html += `
            <li class="page-item ${currentPage === 1 ? 'disabled' : ''}">
                <a class="page-link" href="#" data-page="${currentPage - 1}" ${currentPage === 1 ? 'tabindex="-1"' : ''}>
                    <i class="fa-solid fa-chevron-left"></i>
                </a>
            </li>
        `;

        // Page numbers
        const maxVisible = 5;
        let startPage = Math.max(1, currentPage - Math.floor(maxVisible / 2));
        let endPage = Math.min(totalPages, startPage + maxVisible - 1);

        if (endPage - startPage < maxVisible - 1) {
            startPage = Math.max(1, endPage - maxVisible + 1);
        }

        if (startPage > 1) {
            html += `<li class="page-item"><a class="page-link" href="#" data-page="1">1</a></li>`;
            if (startPage > 2) {
                html += `<li class="page-item disabled"><span class="page-link">...</span></li>`;
            }
        }

        for (let i = startPage; i <= endPage; i++) {
            html += `
                <li class="page-item ${i === currentPage ? 'active' : ''}">
                    <a class="page-link" href="#" data-page="${i}">${i}</a>
                </li>
            `;
        }

        if (endPage < totalPages) {
            if (endPage < totalPages - 1) {
                html += `<li class="page-item disabled"><span class="page-link">...</span></li>`;
            }
            html += `<li class="page-item"><a class="page-link" href="#" data-page="${totalPages}">${totalPages}</a></li>`;
        }

        // Next button
        html += `
            <li class="page-item ${currentPage === totalPages ? 'disabled' : ''}">
                <a class="page-link" href="#" data-page="${currentPage + 1}" ${currentPage === totalPages ? 'tabindex="-1"' : ''}>
                    <i class="fa-solid fa-chevron-right"></i>
                </a>
            </li>
        `;

        html += `
                    </ul>
                </nav>
            </div>
        `;

        container.innerHTML = html;

        // Bind click events
        container.querySelectorAll('.page-link[data-page]').forEach(link => {
            link.addEventListener('click', (e) => {
                e.preventDefault();
                if (!link.closest('.page-item').classList.contains('disabled')) {
                    const page = parseInt(link.dataset.page);
                    if (!isNaN(page)) {
                        this.goToPage(page);
                    }
                }
            });
        });
    }

    /**
     * Show loading state
     */
    showLoading() {
        const tbody = this.config.container?.querySelector(this.config.tableSelector);
        if (tbody) {
            tbody.innerHTML = `
                <tr>
                    <td colspan="100" class="text-center py-4">
                        <i class="fa fa-spinner fa-spin me-2"></i>Đang tải dữ liệu...
                    </td>
                </tr>
            `;
        }
    }

    /**
     * Hide loading state
     */
    hideLoading() {
        // Handled by onDataLoaded callback
    }

    /**
     * Show error message
     */
    showError(message) {
        const tbody = this.config.container?.querySelector(this.config.tableSelector);
        if (tbody) {
            tbody.innerHTML = `
                <tr>
                    <td colspan="100" class="text-center py-4">
                        <div class="alert alert-danger d-inline-block">
                            <i class="fa fa-exclamation-triangle me-2"></i>${message}
                        </div>
                    </td>
                </tr>
            `;
        }
    }

    /**
     * Get current state
     */
    getState() {
        return {
            page: this.state.currentPage,
            limit: this.config.limit,
            total: this.state.totalRecords,
            totalPages: this.state.totalPages,
            filters: { ...this.filters }
        };
    }

    /**
     * Reload với filters hiện tại
     */
    reload() {
        this.loadData();
    }
}

// Export
if (typeof module !== 'undefined' && module.exports) {
    module.exports = Pagination;
}

// Expose to window
if (typeof window !== 'undefined') {
    window.Pagination = Pagination;
}

export default Pagination;
