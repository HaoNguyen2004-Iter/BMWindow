function loadCategory() {
    const $container = $('#category-management');
    if ($container.length === 0) {
        console.error('Không tìm thấy container #category-management để nạp partial.');
        return;
    }
    if ($container.data('loaded')) return;

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
        })
        .fail(function (xhr) {
            $container.html(
                '<div class="alert alert-danger m-3">' +
                'Lỗi tải dữ liệu nhóm ứng dụng (HTTP ' + xhr.status + ').' +
                '</div>'
            );
        });
}

window.loadCategory = loadCategory;

export default loadCategory;
