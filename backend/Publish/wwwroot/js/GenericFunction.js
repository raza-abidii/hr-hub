function searchTable() {
    debugger;
    var input, filter, table, tr, td, i, j, txtValue;
    input = document.getElementById("searchBox");
    filter = input.value.toUpperCase();
    table = document.getElementById("myTable");
    tr = table.getElementsByTagName("tr");

    // Loop through all table rows
    for (i = 1; i < tr.length; i++) {
        tr[i].style.display = "none"; // Hide rows initially
        td = tr[i].getElementsByTagName("td");
        let rowMatched = false;

        // Loop through all cells in the row
        for (j = 0; j < td.length; j++) {
            if (td[j]) {
                txtValue = td[j].textContent || td[j].innerText;
                if (txtValue.toUpperCase().indexOf(filter) > -1) {
                    rowMatched = true;
                }
            }
        }

        // If the row contains the search term, show it and highlight
        debugger
        if (rowMatched) {
            tr[i].style.display = "";
            tr[i].style.backgroundColor = "#ffeb3b"; // Highlight color (light blue)
        } else {
            tr[i].style.backgroundColor = ""; // Reset color if no match
        }
    }
}

function focusonselectcontrol(ctrl) {

    const $select = $(ctrl);

    // Automatically open on focus
    const $container = $select.next('.select2-container');

    $container.find('.select2-selection').on('focus', function () {
        $select.select2('open');
    });

    $select.on('select2:open', function () {
        setTimeout(() => {
            document.querySelector('.select2-container--open .select2-search__field').focus();
        }, 0);
        setTimeout(() => {
            const $searchField = $('.select2-container--open .select2-search__field');
            $searchField.focus();

            // Shift+Tab fix
            $searchField.on('keydown', function (e) {
                //debugger;
                if (e.key === 'Tab' && e.shiftKey) {
                    e.preventDefault(); // Stop Select2 from trapping focus
                    $select.select2('close');

                    // Focus previous focusable element
                    setTimeout(() => {
                        const $prev = $select.closest('form').find(':focusable').filter(function () {
                            return this.compareDocumentPosition($select[0]) & Node.DOCUMENT_POSITION_FOLLOWING;
                        }).last();
                        $prev.focus();
                    }, 0);
                }
            });
        }, 0);

        // Make the container focusable
        $container.find('.select2-selection').attr('tabindex', '0');
    });
}

function setupRefreshButton(btnId, iconId, delay = 2000) {
    debugger
    const btn = document.getElementById(btnId);
    const icon = document.getElementById(iconId);

    if (!btn || !icon) {
        console.warn(`Invalid element(s): btnId=${btnId}, iconId=${iconId}`);
        return;
    }

    btn.addEventListener('click', function () {
        icon.classList.add('spin');

        // Simulate refresh delay
        setTimeout(() => {
            icon.classList.remove('spin');
        }, delay);
    });
}

function limitInput(input, digit) {
    debugger;
    if (input.value.length > digit) {
        input.value = input.value.slice(0, digit);
    }
}