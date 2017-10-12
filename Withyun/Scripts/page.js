
function fixCommentHeight() {
    var ulheight = $('#comments ul').outerHeight(true);
    if (ulheight < 400 - 52 - 50) {
        $('#comments .widget-content').height(ulheight);
        $('#comments').css('padding-bottom', 0).css('min-height', ulheight + 30 + 52 + 50).height(ulheight + 30 + 52 + 50);
    }
}

function initialInvalidBtn() {
    $('.btn-invalidlink').click(function () {
        var invalidlink = $(this);
        requireLogin();
        var linkId = $(this).parent().prev().children('a').attr('data-linkId');
        var linkdata = new Object();
        linkdata["linkId"] = linkId;
        $.post('/linkinvalid/create', linkdata, function (result) {
            if (result.isSuccess) {
                invalidlink.html('<i class="fa fa-chain-broken"></i>已上报');
                invalidlink.prop('disabled', true);
            }
        });
    });
}

function initialPanel() {
    var panel = $('div.tool-panel');
    panel.children().click(function () {
        var linkButton = $(this);
        var icon = linkButton.html().replace(linkButton.text(), '');
        var linkText = linkButton.text().replace('取消', '');
        var linkItem = linkButton.attr('data-name');
        if (linkItem === "share") {
            $('a.bdshare-slide-button').get(0).click();
        } else {
            if (linkItem === "report") {
                requireLogin(showReportModal);
            } else {
                requireLogin();
                var data = {
                    blogId: $('#blogDetail-blogId').val(),
                    blogTitle: $('#blogDetail-blogTitle').val(),
                    distributor: $('#blogDetail-blogUserId').val()
                };
                var url = '/' + linkItem + '/create';
                $.post(url, data, function (result) {
                    if (result.isSuccess) {
                        if (result.entity == "add") {
                            linkButton.html(icon + '取消' + linkText);
                        } else {
                            linkButton.html(icon + linkText);
                        }
                    }
                });
            }
        }

    });
}

function initialReport() {
    $('input[name="ReportType"]').change(function () {
        if ($(this).val() == "4") {
            $('#reportModal input[name="Content"]').parent().parent().slideDown();
        } else {
            $('#reportModal input[name="Content"]').parent().parent().slideUp();
        }
    });
}

function showReportModal() {
    $('#reportModal').modal('show');
}

function initialCopyBtn() {
    var client = new ZeroClipboard($('.btn-copy'));
    client.on('copy', function (event) {
        var linkText = $(event.target).parent().prev().children('a').text();
        event.clipboardData.setData('text/plain', linkText);
    });
    client.on('aftercopy', function (event) {
        $(event.target).attr('title', '复制成功！').tooltip("fixTitle").tooltip('show').attr("title", "复制到剪贴板").tooltip("fixTitle");
    });

    $('.btn-copy').tooltip({
        title: "复制到剪切板",
        trigger: "hover"
    });
}

function getPanelStatus() {
    var data = {
        blogId: $('#blogDetail-blogId').val(),
        blogTitle: $('#blogDetail-blogTitle').val(),
        distributor: $('#blogDetail-blogUserId').val()
    };
    $.getJSON('/blog/getpanelstatus', data, function (result) {
        if (result.isSuccess) {
            var status = result.entity;
            var panel = $('div.tool-panel');
            if (status.HasVoteUp) {
                var link = panel.children('[data-name="votedown"]');
                var text = "取消" + link.text();
                link.html(link.html().replace(link.text(), text));
            }
            if (status.HasVoteUp) {
                link = panel.children('[data-name="voteup"]');
                text = "取消" + link.text();
                link.html(link.html().replace(link.text(), text));
            }
            if (status.HasCollection) {
                link = panel.children('[data-name="collection"]');
                text = "取消" + link.text();
                link.html(link.html().replace(link.text(), text));
            }
        }
    });
}

function reviewSuccess(data) {
    if (data.isSuccess) {
        var review = data.entity;
        var li = $('<li></li>');
        li.append('<img width="30" height="30" src="' + review.ImgUrl + '">');
        var div = $('<div class="bubble"></div>');
        div.append('<a class="user-name" target="_blank" href="/Profile/Index/' + review.UserId + '">' + review.UserName + '</a>');
        div.append('<p class="message">' + review.Content + '</p>');
        div.append('<p class="time"><strong>' + review.Time + '</strong></p>');
        li.append(div);
        var ul = $('#comments ul');
        ul.append(li);
        $('#comments input[name="content"]').val('');
        fixCommentHeight();
        ul.parent().scrollTop(ul.height());
    } else {
        alert(data.entity);
    }
}

function reportSuccess(data) {
    if (data.isSuccess) {
        $('#reportModal').modal('hide');
        $('#reportSuccessModal').modal('show');
    } else {
        alert('网络错误，举报失败，请稍后重试');
    }
}

function loginSuccess(data) {
    if (data.isSuccess) {
        $('#partLoginModal').modal('hide');
        getPanelStatus();
    } else {
        var container = $('#partLoginModal').find("[data-valmsg-summary=true]");
        var list = container.find("ul");
        list.empty();
        container.addClass("validation-summary-errors").removeClass("validation-summary-valid");
        $("<li />").html(data.entity).appendTo(list);
    }
}

function requireLogin(successCallback) {
    $.getJSON('/account/checklogin', function (checkResult) {
        if (checkResult.isSuccess) {
            if (typeof successCallback == 'function') {
                successCallback();
            }
        } else {
            $.get('/account/partiallogin', function (loginResult) {
                $('#partLoginModal').html(loginResult).modal('show');
            });
        }
    });
}

function uploadImages(files, editor, $editable) {
    var filename = false;
    try {
        filename = file['name'];
        //alert(filename);
    } catch (e) { filename = false; }
    if (!filename) { $(".note-alarm").remove(); }
    //以上防止在图片在编辑器内拖拽引发第二次上传导致的提示错误
    var formData = new FormData();
    formData.append("file", files[0]);
    formData.append("blogId", $('#Id').attr('id'));
    $.ajax({
        data: formData,
        type: "POST",
        url: '@Url.Action("FileUpload","Blog")', //图片上传出来的url，返回的是图片上传后的路径，http格式
        contentType: false,
        cache: false,
        processData: false,
        success: function (url) {
            $('#HtmlContent').summernote('editor.insertImage', url);
            $(".note-alarm").html("上传成功,请等待加载");
            setTimeout(function () { $(".note-alarm").remove(); }, 3000);
        },
        error: function () {
            $(".note-alarm").html("上传失败");
            setTimeout(function () { $(".note-alarm").remove(); }, 3000);
        }
    });
}

function pasteSummmernote(e) {
    var bufferText = ((e.originalEvent || e).clipboardData || window.clipboardData).getData('Text');
    var bufferArray = bufferText.split(/\n/);
    e.preventDefault();
    setTimeout(function () {
        for (var i = 0; i < bufferArray.length; i++) {
            if (bufferArray[i] !== "") {
                var node = $('<p>').append(bufferArray[i]);
                $('#HtmlContent').summernote('insertNode', node.get(0));
            }
        }
    }, 10);
}

function submitSummernote() {
    var html = $('#HtmlContent').summernote('code');
    var reg = new RegExp("<p><br></p>", "g");
    html = html.replace(reg, "");
    $('#HtmlContent').val(html);
    $('#tempContent').html(html);
    $('#Content').val($('#tempContent').text());
}

function deleteLink(obj) {
    $(obj).parent().parent().remove();
}

function addLink() {
    $('#copylink').removeClass('hidden').clone().removeAttr('id').insertBefore('#copylink');
    $('#copylink').addClass('hidden');
}

function deleteCollection(id) {
    $.post('/collection/delete/' + id, function (data) {
        if (data.isSuccess) {
            $("tr[data-id='" + id + "']").remove();
        }
    });
}

function addFollowSuccess(result) {
    if (result.isSuccess) {
        $('#addFollowForm').addClass("hidden");
        $('#deleteFollowForm').removeClass("hidden");
    }
}

function deleteFollowSuccess(result) {
    if (result.isSuccess) {
        $('#addFollowForm').removeClass("hidden");
        $('#deleteFollowForm').addClass("hidden");
    }
}

function messageSuccess(data) {
    if (data.isSuccess) {
        $('#messageForm').addClass("hidden");
        $('#messageSuccessInfo').removeClass("hidden");
    }
}

function handleGenerateCode(data) {
    if (data.isSuccess) {
        $('#btnCode').hide();
        $('#NewEmail').attr('disabled', true)
            .next('span').removeClass('field-validation-error').removeClass('field-validation-valid').text(data.entity);
        $('#formEmail').removeClass('hidden');
    } else {
        $('#NewEmail').next('span').removeClass('field-validation-valid').addClass('field-validation-error').text(data.entity);
    }
}

function topRecomment(id) {
    $.post('/recomment/top/' + id, function (data) {
        if (data === "ok") {
            $("tr[data-id='" + id + "']").addClass('success');
        }
    });
}
function cancelTopRecomment(id) {
    $.post('/recomment/untop/' + id, function (data) {
        if (data === "ok") {
            $("tr[data-id='" + id + "']").removeClass('success');
        }
    });
}
function deleteRecomment(id) {
    $.post('/recomment/delete/' + id, function (data) {
        if (data === "ok") {
            $("tr[data-id='" + id + "']").remove();
        }
    });
}