var abp = abp || {};

abp.modals.articleCommentEdit = function () {
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
        
        $('#ArticleLookupOpenButton').on('click', '', function () {
            lastNpDisplayNameId = 'Article_Title';
            lastNpIdId = 'Article_Id';
            _lookupModal.open({
                currentId: $('#Article_Id').val(),
                currentDisplayName: $('#Article_Title').val(),
                serviceMethod: function() {
                    
                    return window.lC.crawler.backOffice.articleComments.articleComments.getArticleLookup;
                }
            });
        });
        
        
    };

    return {
        initModal: initModal
    };
};
