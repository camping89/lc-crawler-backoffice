var abp = abp || {};

abp.modals.productCommentEdit = function () {
    var initModal = function (publicApi, args) {
        var l = abp.localization.getResource("BackOffice");
        
        
        var lastNpIdId = '';
        var lastNpDisplayNameId = '';

        var _lookupModal = new abp.ModalManager({
            viewUrl: abp.appPath + "Shared/LookupModal",
            scriptUrl: "/Pages/Shared/lookupModal.js",
            modalClass: "navigationPropertyLookup"
        });

        $('.lookupCleanButton').on('click', '', function () {
            $(this).parent().find('input').val('');
        });

        _lookupModal.onClose(function () {
            var modal = $(_lookupModal.getModal());
            $('#' + lastNpIdId).val(modal.find('#CurrentLookupId').val());
            $('#' + lastNpDisplayNameId).val(modal.find('#CurrentLookupDisplayName').val());
        });
        
        $('#ProductLookupOpenButton').on('click', '', function () {
            lastNpDisplayNameId = 'Product_Name';
            lastNpIdId = 'Product_Id';
            _lookupModal.open({
                currentId: $('#Product_Id').val(),
                currentDisplayName: $('#Product_Name').val(),
                serviceMethod: function() {
                    
                    return window.lC.crawler.backOffice.productComments.productComments.getProductLookup;
                }
            });
        });
        
        
    };

    return {
        initModal: initModal
    };
};
