CREATE TABLE IF NOT EXISTS fordon_forms (
    "Id" SERIAL PRIMARY KEY,
    first_name TEXT NOT NULL,
    email TEXT NOT NULL,
    reg_nummer TEXT NOT NULL,
    issue_type TEXT NOT NULL,
    message TEXT NOT NULL,
    chat_token TEXT NOT NULL UNIQUE,
    submitted_at TIMESTAMPTZ NOT NULL,
    is_chat_active BOOLEAN NOT NULL,
    company_type TEXT DEFAULT '' NOT NULL
);

CREATE UNIQUE INDEX IF NOT EXISTS "IX_FordonForms_ChatToken" ON fordon_forms (chat_token);

CREATE TABLE IF NOT EXISTS forsakrings_forms (
    "Id" SERIAL PRIMARY KEY,
    first_name TEXT NOT NULL,
    email TEXT NOT NULL,
    insurance_type TEXT NOT NULL,
    issue_type TEXT NOT NULL,
    message TEXT NOT NULL,
    chat_token TEXT NOT NULL UNIQUE,
    submitted_at TIMESTAMPTZ NOT NULL,
    is_chat_active BOOLEAN NOT NULL,
    company_type TEXT DEFAULT '' NOT NULL
);

CREATE UNIQUE INDEX IF NOT EXISTS "IX_ForsakringsForms_ChatToken" ON forsakrings_forms (chat_token);

CREATE TABLE IF NOT EXISTS tele_forms (
    "Id" SERIAL PRIMARY KEY,
    first_name TEXT NOT NULL,
    email TEXT NOT NULL,
    service_type TEXT NOT NULL,
    issue_type TEXT NOT NULL,
    message TEXT NOT NULL,
    chat_token TEXT NOT NULL UNIQUE,
    submitted_at TIMESTAMPTZ NOT NULL,
    is_chat_active BOOLEAN NOT NULL,
    company_type TEXT DEFAULT '' NOT NULL
);

CREATE UNIQUE INDEX IF NOT EXISTS "IX_TeleForms_ChatToken" ON tele_forms (chat_token);

CREATE TABLE IF NOT EXISTS chat_messages (
    id SERIAL PRIMARY KEY,
    sender VARCHAR(100) NOT NULL,
    message TEXT NOT NULL,
    submitted_at TIMESTAMPTZ NOT NULL,
    chat_token TEXT
);

CREATE TABLE IF NOT EXISTS dynamicforms (
    id SERIAL PRIMARY KEY,
    formtype VARCHAR(50) NOT NULL,
    firstname VARCHAR(100) NOT NULL,
    companytype VARCHAR(100) NOT NULL,
    email VARCHAR(255) NOT NULL,
    fields JSONB NOT NULL,
    message TEXT,
    chattoken VARCHAR(100) NOT NULL UNIQUE,
    submittedat TIMESTAMP NOT NULL,
    ischatactive BOOLEAN DEFAULT TRUE NOT NULL
);

CREATE TABLE IF NOT EXISTS role (
    id SERIAL PRIMARY KEY,
    company_role VARCHAR NOT NULL
);

CREATE TABLE IF NOT EXISTS users (
    "Id" SERIAL PRIMARY KEY,
    first_name VARCHAR(50) NOT NULL,
    password VARCHAR(100) NOT NULL,
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP NOT NULL,
    company VARCHAR(50) DEFAULT '' NOT NULL,
    role_id SMALLINT REFERENCES role(id),
    email VARCHAR(255) NOT NULL UNIQUE
);

CREATE TABLE IF NOT EXISTS archived_tickets (
    id SERIAL PRIMARY KEY,
    original_id INTEGER NOT NULL,
    original_table TEXT NOT NULL,
    form_type TEXT NOT NULL,
    first_name TEXT,
    email TEXT,
    issue_type TEXT,
    message TEXT,
    chat_token TEXT NOT NULL,
    submitted_at TIMESTAMPTZ,
    resolved_at TIMESTAMPTZ,
    company_type TEXT,
    resolution_notes TEXT,
    service_type TEXT,
    reg_nummer TEXT,
    insurance_type TEXT
);

create view initial_form_messages (chat_token, sender, message, submitted_at, issue_type, email, form_type) as
SELECT ff.chat_token,
       ff.first_name          AS sender,
       ff.message,
       ff.submitted_at,
       ff.issue_type,
       ff.email,
       'Fordonsservice'::text AS form_type
FROM fordon_forms ff
WHERE ff.is_chat_active = true
UNION ALL
SELECT tf.chat_token,
       tf.first_name         AS sender,
       tf.message,
       tf.submitted_at,
       tf.issue_type,
       tf.email,
       'Tele/Bredband'::text AS form_type
FROM tele_forms tf
WHERE tf.is_chat_active = true
UNION ALL
SELECT f.chat_token,
       f.first_name               AS sender,
       f.message,
       f.submitted_at,
       f.issue_type,
       f.email,
       'Försäkringsärenden'::text AS form_type
FROM forsakrings_forms f
WHERE f.is_chat_active = true;

alter table initial_form_messages
    owner to sebastianholmberg;




CREATE INDEX IF NOT EXISTS idx_archived_chat_token ON archived_tickets (chat_token);
CREATE INDEX IF NOT EXISTS idx_archived_form_type ON archived_tickets (form_type);
CREATE INDEX IF NOT EXISTS idx_archived_resolved_at ON archived_tickets (resolved_at);
