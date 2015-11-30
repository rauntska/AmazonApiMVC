
$(document).ready(function ()
{
    console.log("ready!");
});
//chenge currency without calling the backend method and requery  data.
$("#Currency").change(function () {
    //check if datatable is initialized. This is used when user selects currency before the query is made. 
    //exit method, when not initialized. Continue if initialized.
    var table = $('#example').DataTable();
    if (!$.fn.DataTable.isDataTable('#example')) {
        return false;
    }
    //Get the current currency used. Currency is present in column header. 
    var Columntitle = table.columns(3).header()[0].textContent;
    var titlesplitted = Columntitle.split(" ");
    var currency = titlesplitted[1];
    //get currency from current one to the new one. 
    $.ajax({
        url: 'https://currency-api.appspot.com/api/' + currency + '/' + $('#Currency').val() + '.jsonp',
        dataType: "jsonp",
        data: { amount: '1.00' },
        success: function (response) {
            //update header to new currency
            $(table.column(3).header()).text('Price ' + $('#Currency').val());
            //loop the datatable rows and update data to match new currency.
            table.rows().every(function (rowIdx, tableLoop, rowLoop)
            {
                var d = this.data();
                //round data to 2 decimal places. 
                    d[3] = (d[3] * parseFloat(response.rate)).toFixed(2);
                //if price ==0 , clear the data. This means that amazon does not provide information about the price. 
                if (d[3] == 0) { d[3] = "" };
                this.invalidate(); 
            });

        }
    });
});
$('#SearchButton').click(function () {

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
                                       // default rows per page.
                                        "iDisplayLength": 13,
                                        "aLengthMenu": [[10, 13, 25, 35, 50, 100, -1], [10, 13, 25, 35, 50, 100, "All"]],

                                        "columns": [
                                                  { "title": "Title" },
                                                  { "title": "ProductGroup" },
                                                  { "title": "Author" },                                               
                                                  { "title": "Price " + $('#Currency').val()},
                                        ],
                                        "data": dataSetdynamic,
                                        "autoWidth": true,
                                });
                          
                   
                        }
                        else $('#main').html(jsonData.Error)
                    }
                  
                });
               
            }
        }
    }); 
});