
(function () {

  $(document).ready(function() {

    /*
    # =============================================================================
    #   Navbar scroll animation
    # =============================================================================
    */

    $(".page-header-fixed .navbar.scroll-hide").mouseover(function() {
      $(".page-header-fixed .navbar.scroll-hide").removeClass("closed");
      return setTimeout((function() {
        return $(".page-header-fixed .navbar.scroll-hide").css({
          overflow: "visible"
        });
      }), 150);
    });
    $(function() {
      var delta, lastScrollTop;
      lastScrollTop = 0;
      delta = 50;
      return $(window).scroll(function(event) {
        var st;
        st = $(this).scrollTop();
        if (Math.abs(lastScrollTop - st) <= delta) {
          return;
        }
        if (st > lastScrollTop) {
          $('.page-header-fixed .navbar.scroll-hide').addClass("closed");
        } else {
          $('.page-header-fixed .navbar.scroll-hide').removeClass("closed");
        }
        return lastScrollTop = st;
      });
    });
    /*
    # =============================================================================
    #   Mobile Nav
    # =============================================================================
    */

    $('.navbar-toggle').click(function() {
      return $('body, html').toggleClass("nav-open");
    });

    /*
    # =============================================================================
    #   Bootstrap Tabs
    # =============================================================================
    */

    $("#myTab a:last").tab("show");
    /*
    # =============================================================================
    #   Bootstrap Popover
    # =============================================================================
    */

    $(".popover-trigger").popover();
    /*
    # =============================================================================
    #   Bootstrap Tooltip
    # =============================================================================
    */

    $(".tooltip-trigger").tooltip();


    /*
    # =============================================================================
    #   Popover JS
    # =============================================================================
    */

    $('#popover').popover();
    /*
    # =============================================================================
    #   Fancybox Modal
    # =============================================================================
    */

    $(".fancybox").fancybox({
      maxWidth: 700,
      height: 'auto',
      fitToView: false,
      autoSize: true,
      padding: 15,
      nextEffect: 'fade',
      prevEffect: 'fade',
      helpers: {
        title: {
          type: "outside"
        }
      }
    });

    /*
    # =============================================================================
    #   WYSIWYG Editor
    # =============================================================================
    */

    if ($('#summernote').length) {
      $('#summernote').summernote({
        height: 300,
        focus: true,
        toolbar: [['style', ['style']], ['style', ['bold', 'italic', 'underline', 'clear']], ['fontsize', ['fontsize']], ['color', ['color']], ['para', ['ul', 'ol', 'paragraph']], ['height', ['height']], ['insert', ['picture', 'link']], ['table', ['table']], ['fullscreen', ['fullscreen']]]
      });
    }
    /*
    # =============================================================================
    #   Typeahead
    # =============================================================================
    */

    if ($('.typeahead').length) {
      $(".states.typeahead").typeahead({
        name: "states",
        local: ["Alabama", "Alaska", "Arizona", "Arkansas", "California", "Colorado", "Connecticut", "Delaware", "Florida", "Georgia", "Hawaii", "Idaho", "Illinois", "Indiana", "Iowa", "Kansas", "Kentucky", "Louisiana", "Maine", "Maryland", "Massachusetts", "Michigan", "Minnesota", "Mississippi", "Missouri", "Montana", "Nebraska", "Nevada", "New Hampshire", "New Jersey", "New Mexico", "New York", "North Dakota", "North Carolina", "Ohio", "Oklahoma", "Oregon", "Pennsylvania", "Rhode Island", "South Carolina", "South Dakota", "Tennessee", "Texas", "Utah", "Vermont", "Virginia", "Washington", "West Virginia", "Wisconsin", "Wyoming"]
      });
      $(".countries.typeahead").typeahead({
        name: "countries",
        local: ["Andorra", "United Arab Emirates", "Afghanistan", "Antigua and Barbuda", "Anguilla", "Albania", "Armenia", "Angola", "Antarctica", "Argentina", "American Samoa", "Austria", "Australia", "Aruba", "Ã…land", "Azerbaijan", "Bosnia and Herzegovina", "Barbados", "Bangladesh", "Belgium", "Burkina Faso", "Bulgaria", "Bahrain", "Burundi", "Benin", "Saint BarthÃ©lemy", "Bermuda", "Brunei", "Bolivia", "Bonaire", "Brazil", "Bahamas", "Bhutan", "Bouvet Island", "Botswana", "Belarus", "Belize", "Canada", "Cocos [Keeling] Islands", "Congo", "Central African Republic", "Republic of the Congo", "Switzerland", "Ivory Coast", "Cook Islands", "Chile", "Cameroon", "China", "Colombia", "Costa Rica", "Cuba", "Cape Verde", "Curacao", "Christmas Island", "Cyprus", "Czechia", "Germany", "Djibouti", "Denmark", "Dominica", "Dominican Republic", "Algeria", "Ecuador", "Estonia", "Egypt", "Western Sahara", "Eritrea", "Spain", "Ethiopia", "Finland", "Fiji", "Falkland Islands", "Micronesia", "Faroe Islands", "France", "Gabon", "United Kingdom", "Grenada", "Georgia", "French Guiana", "Guernsey", "Ghana", "Gibraltar", "Greenland", "Gambia", "Guinea", "Guadeloupe", "Equatorial Guinea", "Greece", "South Georgia and the South Sandwich Islands", "Guatemala", "Guam", "Guinea-Bissau", "Guyana", "Hong Kong", "Heard Island and McDonald Islands", "Honduras", "Croatia", "Haiti", "Hungary", "Indonesia", "Ireland", "Israel", "Isle of Man", "India", "British Indian Ocean Territory", "Iraq", "Iran", "Iceland", "Italy", "Jersey", "Jamaica", "Jordan", "Japan", "Kenya", "Kyrgyzstan", "Cambodia", "Kiribati", "Comoros", "Saint Kitts and Nevis", "North Korea", "South Korea", "Kuwait", "Cayman Islands", "Kazakhstan", "Laos", "Lebanon", "Saint Lucia", "Liechtenstein", "Sri Lanka", "Liberia", "Lesotho", "Lithuania", "Luxembourg", "Latvia", "Libya", "Morocco", "Monaco", "Moldova", "Montenegro", "Saint Martin", "Madagascar", "Marshall Islands", "Macedonia", "Mali", "Myanmar [Burma]", "Mongolia", "Macao", "Northern Mariana Islands", "Martinique", "Mauritania", "Montserrat", "Malta", "Mauritius", "Maldives", "Malawi", "Mexico", "Malaysia", "Mozambique", "Namibia", "New Caledonia", "Niger", "Norfolk Island", "Nigeria", "Nicaragua", "Netherlands", "Norway", "Nepal", "Nauru", "Niue", "New Zealand", "Oman", "Panama", "Peru", "French Polynesia", "Papua New Guinea", "Philippines", "Pakistan", "Poland", "Saint Pierre and Miquelon", "Pitcairn Islands", "Puerto Rico", "Palestine", "Portugal", "Palau", "Paraguay", "Qatar", "RÃ©union", "Romania", "Serbia", "Russia", "Rwanda", "Saudi Arabia", "Solomon Islands", "Seychelles", "Sudan", "Sweden", "Singapore", "Saint Helena", "Slovenia", "Svalbard and Jan Mayen", "Slovakia", "Sierra Leone", "San Marino", "Senegal", "Somalia", "Suriname", "South Sudan", "SÃ£o TomÃ© and PrÃ­ncipe", "El Salvador", "Sint Maarten", "Syria", "Swaziland", "Turks and Caicos Islands", "Chad", "French Southern Territories", "Togo", "Thailand", "Tajikistan", "Tokelau", "East Timor", "Turkmenistan", "Tunisia", "Tonga", "Turkey", "Trinidad and Tobago", "Tuvalu", "Taiwan", "Tanzania", "Ukraine", "Uganda", "U.S. Minor Outlying Islands", "United States", "Uruguay", "Uzbekistan", "Vatican City", "Saint Vincent and the Grenadines", "Venezuela", "British Virgin Islands", "U.S. Virgin Islands", "Vietnam", "Vanuatu", "Wallis and Futuna", "Samoa", "Kosovo", "Yemen", "Mayotte", "South Africa", "Zambia", "Zimbabwe"]
      });
    }

    /*
    # =============================================================================
    #   Drag and drop files
    # =============================================================================
    */

    $(".single-file-drop").each(function() {
      var $dropbox;
      $dropbox = $(this);
      if (typeof window.FileReader === "undefined") {
        $("small", this).html("File API & FileReader API not supported").addClass("text-danger");
        return;
      }
      this.ondragover = function() {
        $dropbox.addClass("hover");
        return false;
      };
      this.ondragend = function() {
        $dropbox.removeClass("hover");
        return false;
      };
      return this.ondrop = function(e) {
        var file, reader;
        e.preventDefault();
        $dropbox.removeClass("hover").html("");
        file = e.dataTransfer.files[0];
        reader = new FileReader();
        reader.onload = function(event) {
          return $dropbox.append($("<img>").attr("src", event.target.result));
        };
        reader.readAsDataURL(file);
        return false;
      };
    });

    /*
    # =============================================================================
    #   Login/signup animation
    # =============================================================================
    */

    $(window).load(function() {
      return $(".login-container").addClass("active");
    });

    /*
    # =============================================================================
    #   Timeline animation
    # =============================================================================
    */

    timelineAnimate = function(elem) {
      return $(".timeline.animated li").each(function(i) {
        var bottom_of_object, bottom_of_window;
        bottom_of_object = $(this).position().top + $(this).outerHeight();
        bottom_of_window = $(window).scrollTop() + $(window).height();
        if (bottom_of_window > bottom_of_object) {
          return $(this).addClass("active");
        }
      });
    };
    timelineAnimate();
    $(window).scroll(function() {
      return timelineAnimate();
    });
    /*
    # =============================================================================
    #   Input placeholder fix
    # =============================================================================
    */

    if (!Modernizr.input.placeholder) {
      $("[placeholder]").focus(function() {
        var input;
        input = $(this);
        if (input.val() === input.attr("placeholder")) {
          input.val("");
          return input.removeClass("placeholder");
        }
      }).blur(function() {
        var input;
        input = $(this);
        if (input.val() === "" || input.val() === input.attr("placeholder")) {
          input.addClass("placeholder");
          return input.val(input.attr("placeholder"));
        }
      }).blur();
      $("[placeholder]").parents("form").submit(function() {
        return $(this).find("[placeholder]").each(function() {
          var input;
          input = $(this);
          if (input.val() === input.attr("placeholder")) {
            return input.val("");
          }
        });
      });
    }
      var count;
      if ($.cookie('NotificationCount') == null) {
          $.getJSON('/Notification/GetUnReadCount', function (result) {
              if (result.isSuccess) {
                  count = result.entity;
              } else {
                  count = 0;
              }

              var now = new Date();
              var expireTime = new Date(now.getTime() + 1 * 60 * 60 * 1000);
              $.cookie('NotificationCount', count, { path: "/", expires: expireTime });
              if (count != 0) {
                  $('#notificationDropdown').append('<p class="counter">' + count + '</p>');
              }
          });
      } else {
          count = $.cookie('NotificationCount');
          if (count != "0") {
              $('#notificationDropdown').append('<p class="counter">' + count + '</p>');
          }
      }

      $('#notificationDropdown').click(function() {
          $.getJSON('/Notification/GetTopList', function (result) {
              if (result.isSuccess) {
                  var entity = result.entity;
                  for (var index = 0; index < entity.length; index++) {
                      var notice = entity[index];
                      var p = $('<a>');
                      p.append(notice.Title);
                      var li = $('<li>');
                      li.append(p);
                      var ul = $('#notificationDropdown').next('ul');
                      ul.append(li);
                      $('#notificationDropdown p.counter').remove();
                      var now = new Date();
                      var expireTime = new Date(now.getTime() + 1 * 60 * 60 * 1000);
                      $.cookie('NotificationCount', 0, { path: "/", expires: expireTime });
                  }
              }
          });
      });

  });

}).call(this);
