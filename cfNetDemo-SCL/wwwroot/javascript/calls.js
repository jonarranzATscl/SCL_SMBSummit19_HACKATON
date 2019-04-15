$(document).ready(function () {
	//Get Items from Cloud Platform DB
	$.get("/SelectItems", function (json) {
		displayItemsSQL(json);
	});

	$('#sync').on('click', function () {
		$('#sync i').addClass("fa-spin");
		$('#sync').prop('disabled', true)
		$.post("/Sync", function () {
			setTimeout(function(){
				$('#sync i').removeClass("fa-spin");
				$('#sync').prop('disabled', false);
				location.reload()}, 1500)
		});
	});

});


function displayItemsSQL(items) {
	$("#resultTableSQL tbody").empty();
	//Lines	
	for (var i = 0; i < items.length; i++) {
		$("#resultTableSQL tbody").append(
			"<tr>" +
			"<td>" + (i + 1) + "</td>" +
			"<td>" + items[i].code + "</td>" +
			"<td>" + items[i].name + "</td>" +
			"<td>" + items[i].integrated + "</td>" +
			"</tr>");
	}
}