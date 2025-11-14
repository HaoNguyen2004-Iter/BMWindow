class Pagination {
    constructor(options = {}) {
        this.config = {
            apiUrl: options.apiUrl || '',
            container: options.container || null,
            replaceSelector: options.replaceSelector || null, // NEW
            filterRowSelector: options.filterRowSelector || '.filter-row',
            paginationLinkSelector: options.paginationLinkSelector || 'a.pg-link, .pagination a.page-link, a[data-ajax="true"]',
            page: options.page || 1,
            limit: options.limit || 20,
            orderBy: options.orderBy || '',
            orderType: options.orderType || 'asc',
            texts: {
                loading: (options.texts?.loading) || 'Đang tải dữ liệu...',
                error: (options.texts?.error) || 'Không thể tải dữ liệu. Vui lòng thử lại.'
            },
            onBeforeLoad: options.onBeforeLoad || null,
            onAfterLoad: options.onAfterLoad || null
        };
        this.state = { currentPage: this.config.page, loading: false };
        this.filters = { ...(options.filters || {}) };
        if (!this.config.container) console.error('Pagination: container is required');
    }

    _getTarget() {
        if (!this.config.container) return null;
        if (!this.config.replaceSelector) return this.config.container;
        return this.config.container.querySelector(this.config.replaceSelector) || this.config.container;
    }

    // Build the request URL from apiUrl + filters + paging + sorting
    buildUrl() {
        const base = this.config.apiUrl || '';
        const u = new URL(base, window.location.origin); // keeps existing query (e.g. ?part=table)
        const params = u.searchParams;

        // merge filters
        Object.entries(this.filters || {}).forEach(([k, v]) => {
            if (v === null || v === undefined || v === '') return;
            if (Array.isArray(v)) {
                params.delete(k);
                v.forEach(item => params.append(k, item));
            } else {
                params.set(k, v);
            }
        });

        params.set('page', String(this.state.currentPage));
        if (this.config.limit) params.set('pageSize', String(this.config.limit));
        if (this.config.orderBy) params.set('orderBy', this.config.orderBy);
        if (this.config.orderType) params.set('orderType', this.config.orderType);

        return `${u.pathname}?${params.toString()}`;
    }

    async loadData() {
        if (!this.config.container || this.state.loading) return;
        const url = this.buildUrl();
        await this._loadUrlIntoContainer(url);
    }

    reload() { this.loadData(); }

    async loadUrl(url) {
        if (!this.config.container || this.state.loading) return;
        try {
            const u = new URL(url, window.location.origin);
            const p = parseInt(u.searchParams.get('page') || u.searchParams.get('Page') || this.state.currentPage, 10);
            if (!isNaN(p)) this.state.currentPage = p;
        } catch { }
        await this._loadUrlIntoContainer(url);
    }

    async _loadUrlIntoContainer(url) {
        this.state.loading = true;
        this._showLoading();
        try {
            this.config.onBeforeLoad?.({ page: this.state.currentPage, limit: this.config.limit, filters: { ...this.filters } });
            const resp = await fetch(url, { headers: { 'X-Requested-With': 'XMLHttpRequest' } });
            if (!resp.ok) throw new Error(`HTTP ${resp.status}`);
            const html = await resp.text();
            const target = this._getTarget();
            if (target) target.innerHTML = html;
            this._bindFilterEvents();
            this._bindPagerLinks();
            this.config.onAfterLoad?.({ page: this.state.currentPage, limit: this.config.limit, filters: { ...this.filters } });
        } catch (err) {
            console.error('Pagination load error:', err);
            this._showError(this.config.texts.error);
        } finally {
            this.state.loading = false;
        }
    }

    _bindFilterEvents() {
        if (!this.config.container) return;
        const filterInputs = this.config.container
            .querySelectorAll(`${this.config.filterRowSelector} input, ${this.config.filterRowSelector} select`);
        filterInputs.forEach(input => {
            const filterKey = input.dataset.filter || input.name || 'keyword';
            const debounceMs = parseInt(input.dataset.debounce || '400', 10);
            const clone = input.cloneNode(true);
            input.parentNode.replaceChild(clone, input);
            let debounceTimer;
            if (clone.tagName === 'SELECT' || clone.type === 'date') {
                clone.addEventListener('change', e => this.setFilter(filterKey, e.target.value));
            } else {
                clone.addEventListener('input', e => {
                    clearTimeout(debounceTimer);
                    debounceTimer = setTimeout(() => this.setFilter(filterKey, e.target.value), isNaN(debounceMs) ? 400 : debounceMs);
                });
            }
        });
    }

    _bindPagerLinks() {
        if (!this.config.container) return;
        const links = this.config.container.querySelectorAll(this.config.paginationLinkSelector);
        links.forEach(a => {
            const clone = a.cloneNode(true);
            a.parentNode.replaceChild(clone, a);
            clone.addEventListener('click', e => {
                const dp = clone.getAttribute('data-page');
                if (dp) {
                    e.preventDefault();
                    this.goToPage(dp); // uses buildUrl()
                    return;
                }
                const href = clone.getAttribute('href') || '';
                if (href && href !== '#' && !clone.hasAttribute('data-fullredirect')) {
                    e.preventDefault();
                    this.loadUrl(href);
                }
            });
        });
    }

    goToPage(page) {
        const pageNum = parseInt(page, 10);
        if (isNaN(pageNum) || pageNum < 1 || pageNum === this.state.currentPage) return;
        this.state.currentPage = pageNum;
        this.loadData();
    }

    setLimit(limit) {
        const l = parseInt(limit, 10);
        if (!isNaN(l) && l > 0) {
            this.config.limit = l;
            this.state.currentPage = 1;
            this.loadData();
        }
    }

    setFilter(key, value) {
        if (value === null || value === undefined || value === '') delete this.filters[key];
        else this.filters[key] = value;
        this.state.currentPage = 1;
        this.loadData();
    }

    setFilters(filters) {
        this.filters = { ...filters };
        this.state.currentPage = 1;
        this.loadData();
    }

    clearFilters() {
        this.filters = {};
        this.state.currentPage = 1;
        if (this.config.container) {
            this.config.container
                .querySelectorAll(`${this.config.filterRowSelector} input, ${this.config.filterRowSelector} select`)
                .forEach(input => { input.value = ''; });
        }
        this.loadData();
    }

    _showLoading() {
        const target = this._getTarget() || this.config.container;
        if (!target) return;
        target.innerHTML = `
            <div class="p-3 text-center text-muted">
                <i class="fa fa-spinner fa-spin me-2"></i>${this.config.texts.loading}
            </div>`;
    }

    _showError(message) {
        const target = this._getTarget() || this.config.container;
        if (!target) return;
        target.innerHTML = `
            <div class="alert alert-danger m-3">
                <i class="fa fa-exclamation-triangle me-2"></i>${message}
            </div>`;
    }
}
if (typeof window !== 'undefined') window.Pagination = Pagination;
export default Pagination;