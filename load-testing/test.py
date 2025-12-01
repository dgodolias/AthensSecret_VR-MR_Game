import pandas as pd
import os
import sys

# 1. Φόρτωση του αρχείου CSV
# Προσπαθούμε πρώτα να φορτώσουμε το αρχείο από τα ακόλουθα μέρη (σειρά ελέγχου):
#  1) το μονοπάτι που πέρασε ο χρήστης ως παράμετρος CLI
#  2) το τρέχον working directory
#  3) το directory όπου βρίσκεται αυτό το script (ασφαλέστερο για relative εκτέλεση)

DEFAULT_FILENAME = 'vrpark_sessions.csv'

def find_csv(filename: str) -> str:
	# 1) CLI override
	if filename and os.path.isabs(filename) and os.path.exists(filename):
		return filename

	# 2) path relative to current working directory
	cwd_path = os.path.join(os.getcwd(), filename)
	if os.path.exists(cwd_path):
		return cwd_path

	# 3) path relative to this script
	script_dir = os.path.dirname(os.path.abspath(__file__))
	script_path = os.path.join(script_dir, filename)
	if os.path.exists(script_path):
		return script_path

	# 4) not found -> raise helpful error
	raise FileNotFoundError(
		f"CSV not found. Checked: (cwd) {cwd_path}, (script) {script_path}. "
		f"Pass a path as first arg to the script, e.g. `python test.py {script_path}`."
	)

# Allow running as: python test.py [path/to/vrpark_sessions.csv]
filename = sys.argv[1] if len(sys.argv) > 1 else DEFAULT_FILENAME
csv_path = find_csv(filename)
df = pd.read_csv(csv_path, dtype={'Eye Tracking Sequence': str})

# 2. Δημιουργία του νέου πεδίου 'sequence_length'
# Καθαρίζουμε τα whitespace (spaces, tabs, newlines) από τη στήλη
# πριν μετρήσουμε το μήκος, ώστε αυτά να μην μετράνε ως χαρακτήρες.
# Χρησιμοποιούμε regex replacement για να αφαιρέσουμε όλα τα \s.
# Θέλουμε επίσης να διατηρήσουμε τα missing values ως pd.NA και
# το τελικό dtype να είναι nullable integer 'Int64'.
cleaned = df['Eye Tracking Sequence'].astype('string').str.replace(r'\s+', '', regex=True)
# Treat empty strings (after stripping) as missing
cleaned = cleaned.replace('', pd.NA)
seq_len = cleaned.str.len()
# Convert to nullable integer (preserves pd.NA for missing)
df['sequence_length'] = seq_len.astype('Int64')

# (Προαιρετικό) Γέμισμα των κενών τιμών (NaN) με 0, αν θέλεις να μην υπάρχουν κενά στο length
# df['sequence_length'] = df['sequence_length'].fillna(0)

# 3. Εμφάνιση των πρώτων 5 γραμμών για επαλήθευση
print(df[['Session ID', 'Eye Tracking Sequence', 'sequence_length']].head())

# 4. Αποθήκευση του νέου αρχείου
df.to_csv('vrpark_sessions_updated.csv', index=False)

print("Το αρχείο ενημερώθηκε επιτυχώς!")