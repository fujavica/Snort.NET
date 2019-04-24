// Content opening/closing
$(document).ready(function () {
    $('.content-toggle').on('click', function () {
        $(this).parents('.widget:first').find('.widget-content').slideToggle();
    });
});

// Sidebar search
(function () {
    $(document).on('keyup', '#SearchInput', function () {
        var searches = this.value.toLowerCase().split(' ');
        var menus = $('.sidebar li');

        for (var i = 0; i < menus.length; i++) {
            var menu = $(menus[i]);
            var isMatch = true;

            var menuWords = menu.text().toLowerCase().split(' ');
            for (var j = 0; j < searches.length; j++) {
                var hasMatch = false;
                for (var k = 0; k < menuWords.length; k++) {
                    if (menuWords[k].indexOf(searches[j]) >= 0) {
                        hasMatch = true;
                    }
                }

                if (!hasMatch) {
                    isMatch = false;
                }
            }

            if (isMatch) {
                menu.show(500);
            } else {
                menu.hide(500);
            }
        }
    });
}());

// Mvc.Grid binding
$(function () {
    $('.mvc-grid').mvcgrid();
});
