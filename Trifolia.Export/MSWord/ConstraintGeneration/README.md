The FormattedConstraint class is versioned so that previous publications of an implementation guide
can use the same logic to generate the constraints as when they originally published.

The FormattedConstraint class itself (unversioned) is used for anything published prior to 2014-04-15.
For implementation guides that have not been published yet, the latest version of the class is used (the most recent date).

To add a new version of the FormattedConstraint class:
* Copy the most recent version of the FormattedConstraint class
* Update the copy with the correct date
* Reference the new version in the FormattedConstraintFactory class
* Update the copy with the changes desired