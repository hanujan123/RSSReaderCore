//search funncctionality
var minSearchLen = 2;
$("#strSearchInput").on("change input", function () {
		var searchStr = $("#strSearchInput")[0].value.toLowerCase();
		if (searchStr.length >= minSearchLen) {
				var articles = $(".SearchContainer .searchElement .str")
				for (var i = 0; i < articles.length; i++) {
						if (articles[i].innerHTML.toLowerCase().includes(searchStr))
								$(articles[i].parentNode).show();
						else
								$(articles[i].parentNode).hide();
				}
		}
		else {
				$(".SearchContainer .searchElement").show();
		}
});