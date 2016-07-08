function decode_base64(s) {
    var e = {}, i, k, v = [], r = '', w = String.fromCharCode;
    var n = [[65, 91], [97, 123], [48, 58], [43, 44], [47, 48]];

    for (z in n) {
        for (i = n[z][0]; i < n[z][1]; i++) {
            v.push(w(i));
        }
    }
    for (i = 0; i < 64; i++) {
        e[v[i]] = i;
    }

    for (i = 0; i < s.length; i += 72) {
        var b = 0, c, x, l = 0, o = s.substring(i, i + 72);
        for (x = 0; x < o.length; x++) {
            c = e[o.charAt(x)];
            b = (b << 6) + c;
            l += 6;
            while (l >= 8) {
                r += w((b >>> (l -= 8)) % 256);
            }
        }
    }
    return r;
};

var ImportViewModel = function () {
    var self = this;

    self.ImportFileInfo = ko.observable();
    self.ImportContent = ko.observable('');
    self.ImportResults = ko.observable();
    self.IsImporting = ko.observable(false);

    self.DisableImportButton = ko.computed(function () {
        return self.IsImporting() || !self.ImportContent();
    });

    self.GetTemplateStatus = function (template) {
        if (template.Status == 'Unchanged') {
            var constraintChanged = _.find(template.Constraints, function (constraint) {
                if (constraint.Status == 'Unchanged') {
                    var sampleChanged = _.find(constraint.Samples, function(sample) {
                        return sample.Status != 'Unchanged';
                    });

                    if (sampleChanged) {
                        return true;
                    }
                }

                return true;
            });

            if (constraintChanged) {
                return 'Modified';
            }

            var sampleChanged = _.find(template.Samples, function (sample) {
                return sample.Status != 'Unchanged';
            });

            if (sampleChanged) {
                return 'Modified';
            }

            return 'Unchanged';
        }

        return template.Status;
    };

    self.Import = function () {
        var importContent = decode_base64(self.ImportContent());

        self.IsImporting(true);

        $.ajax({
            url: '/api/Import/Trifolia',
            method: 'POST',
            data: importContent,
            contentType: 'application/xml',
            success: function (results) {
                _.each(results.Templates, function (template) {
                    template.Expanded = ko.observable(false);
                });

                self.ImportResults(results);
                self.IsImporting(false);
            },
            error: function (err, err1, err2) {
                self.IsImporting(false);
                alert('An error ocurred while importing');
            }
        });
    };
};