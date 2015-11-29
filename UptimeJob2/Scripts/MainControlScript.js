// A $( document ).ready() block.
$(document).ready(function ()
{
    console.log("ready!");
});


$('#SearchButton').click(function () {

    //window.location.href = '/SearchProduct/Search';
    $('#main').empty();
    $('#main').append('<h3><img src="Images/ajax-loader.gif" align="center" /> Just a moment... </h3>')
    $.ajax({

        url: 'https://currency-api.appspot.com/api/GBP/' + $('#Currency').val() + '.jsonp',
        dataType: "jsonp",
        data: { amount: '1.00' },
        success: function (response) {
            if (response.success) {              
                $.ajax({
                    type: 'GET',
                    url: '/SearchProduct/Search',                   
                    data: "&Keyword=" + $('#SearchInput').val() + "&Rate=" + parseFloat(response.rate).toFixed(2) + "&SearchIndex=" + $('#Category').val(),
                    dataType: 'json',
                    success: function (jsonData)
                    {

                        if (jsonData.Error == "")
                        {
                            var dataSetdynamic = [[]];
                            for (i = 0; i < (jsonData._SearchResultsList.length) ; i++)//rows
                            {//set product and SN number
                                dataSetdynamic[i] = []
                                dataSetdynamic[i][0] = (jsonData._SearchResultsList[i].Title)
                                dataSetdynamic[i][1] = (jsonData._SearchResultsList[i].ProductGroup)
                                dataSetdynamic[i][2] = (jsonData._SearchResultsList[i].Author)
                                dataSetdynamic[i][3] = (jsonData._SearchResultsList[i].Price)
                            }


                            $('#main').html('<table cellpadding="1" cellspacing="0" border="1" class="display" id="example"></table>');
                            $('#example').dataTable
                                ({
                                    
                                        "bPaginate": true,
                                        "iDisplayLength": 13,
                                        "aLengthMenu": [[10, 13, 25, 35, 50, 100, -1], [10, 13, 25, 35, 50, 100, "All"]],

                                        "columns": [
                                                  { "title": "Title" },
                                                  { "title": "ProductGroup" },
                                                  { "title": "Author" },                                               
                                                  { "title": "Price[" + $('#Currency').val() + "]" },
                                        ],
                                        "data": dataSetdynamic,


                                        "autoWidth": false,
                                });
                          
                   
                        }
                        else $('#main').html(jsonData.Error)
                    }
                  
                });
               
            }
        }
    }); 
});