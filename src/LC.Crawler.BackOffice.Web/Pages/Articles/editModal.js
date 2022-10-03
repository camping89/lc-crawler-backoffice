var abp = abp || {};

abp.modals.articleEdit = function () {
    var initModal = function (publicApi, args) {
        var l = abp.localization.getResource("BackOffice");
        
        
        
        
        
        publicApi.onOpen(function () {
            $('#CategoryLookup').select2({
                dropdownParent: $('#ArticleEditModal'),
                ajax: {
                    url: abp.appPath + 'api/app/articles/category-lookup',
                    type: 'GET',
                    data: function (params) {
                        return { filter: params.term, maxResultCount: 10 }
                    },
                    processResults: function (data) {
                        var mappedItems = _.map(data.items, function (item) {
                            return { id: item.id, text: item.displayName };
                        });

                        return { results: mappedItems };
                    }
                }
            });
        });

        var getNewCategoryIndex = function () {
            var idTds = $($(document).find("#CategoryTableRows")).find('td[name="id"]');

            if (idTds.length === 0){
                return 0;
            }

            return parseInt($(idTds[idTds.length -1]).attr("index")) +1;
        };

        var getCategoryIds = function () {
            var ids = [];
            var idTds = $("#CategoryTableRows td[name='id']");

            for(var i = 0; i< idTds.length; i++){
                ids.push(idTds[i].innerHTML.trim())
            }

            return ids;
        };

        $('#AddCategoryButton').on('click', '', function(){
            var $select = $('#CategoryLookup');
            var id = $select.val();
            var existingIds = getCategoryIds();
            if (!id || id === '' || existingIds.indexOf(id) >= 0){
                return;
            }

            $("#CategoryTable").show();

            var displayName = $select.find('option').filter(':selected')[0].innerHTML;

            var newIndex = getNewCategoryIndex();

            $("#CategoryTableRows").append(
                '                                <tr style="text-align: center; vertical-align: middle;" index="'+newIndex+'">\n' +
                '                                    <td style="display: none" name="id" index="'+newIndex+'">'+id+'</td>\n' +
                '                                    <td style="display: none">' +
                '                                        <input value="'+id+'" id="SelectedCategoryIds['+newIndex+']" name="SelectedCategoryIds['+newIndex+']"/>\n' +
                '                                    </td>\n' +
                '                                    <td style="text-align: left">'+displayName+'</td>\n' +
                '                                    <td style="text-align: right">\n' +
                '                                        <button class="btn btn-danger btn-sm text-light categoryDeleteButton" index="'+newIndex+'"> <i class="fa fa-trash"></i> </button>\n' +
                '                                    </td>\n' +
                '                                </tr>'
            );
        });

        $(document).on('click', '.categoryDeleteButton', function (e) {
            e.preventDefault();
            var index = $(this).attr("index");
            $("#CategoryTableRows").find('tr[index='+index+']').remove();

            setTimeout(
                function()
                {
                    var rows = $(document).find("#CategoryTableRows").find("tr");

                    if (rows.length === 0){
                        $("#CategoryTable").hide();
                    }

                    for (var i=0; i<rows.length; i++){
                        $(rows[i]).attr('index', i);
                        $(rows[i]).find('th[scope="Row"]').empty();
                        $(rows[i]).find('th[scope="Row"]').append(i+1);
                        $($(rows[i]).find('td[name="id"]')).attr('index', i);
                        $($(rows[i]).find('input')).attr('id', 'SelectedCategoryIds['+i+']');
                        $($(rows[i]).find('input')).attr('name', 'SelectedCategoryIds['+i+']');
                        $($(rows[i]).find('button')).attr('index', i);
                    }
                }, 200);
        });
    };

    return {
        initModal: initModal
    };
};
