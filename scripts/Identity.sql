CREATE SCHEMA identity
    AUTHORIZATION postgres;
--------------------------------------------------
CREATE TABLE identity.provider
(
    id smallint NOT NULL,
    name character varying(20) NOT NULL,
    CONSTRAINT pk_provider PRIMARY KEY (id)
);

ALTER TABLE IF EXISTS identity.provider
    OWNER to postgres;

INSERT INTO identity.provider (id, name) VALUES (0, 'Local');
--------------------------------------------------
CREATE TABLE identity.account
(
    id uuid NOT NULL,
    provider_id smallint NOT NULL DEFAULT 0,
    email character varying(256) NOT NULL,
    verified boolean NOT NULL DEFAULT false,
    verified_on timestamp with time zone,
    created_on timestamp with time zone NOT NULL DEFAULT (now() at time zone 'utc'),
    updated_on timestamp with time zone,
    CONSTRAINT pk_account PRIMARY KEY (id),
    CONSTRAINT u_account UNIQUE (provider_id, email),
    CONSTRAINT fk_account_provider FOREIGN KEY (provider_id)
        REFERENCES identity.provider (id)
);

ALTER TABLE IF EXISTS identity.account
    OWNER to postgres;
--------------------------------------------------
CREATE TABLE identity.account_audit
(
    id bigserial NOT NULL,
    account_id uuid NOT NULL,
    email character varying(256) NOT NULL,
    updated_on timestamp with time zone NOT NULL DEFAULT (now() at time zone 'utc'),
    CONSTRAINT pk_account_audit PRIMARY KEY (id),
    CONSTRAINT fk_account_audit_account FOREIGN KEY (account_id)
        REFERENCES identity.account (id)
);

ALTER TABLE IF EXISTS identity.account_audit
    OWNER to postgres;
--------------------------------------------------
CREATE TABLE identity.password
(
    account_id uuid NOT NULL,
    hash character varying(60) NOT NULL,
    updated_on timestamp with time zone,
    CONSTRAINT pk_password PRIMARY KEY (account_id),
    CONSTRAINT fk_password_account FOREIGN KEY (account_id)
        REFERENCES identity.account (id)
);

ALTER TABLE IF EXISTS identity.password
    OWNER to postgres;
--------------------------------------------------
CREATE TABLE identity.password_audit
(
    id bigserial NOT NULL,
    account_id uuid NOT NULL,
    hash character varying(60) NOT NULL,
    updated_on timestamp with time zone NOT NULL DEFAULT (now() at time zone 'utc'),
    CONSTRAINT pk_password_audit PRIMARY KEY (id),
    CONSTRAINT fk_password_audit_account FOREIGN KEY (account_id)
        REFERENCES identity.account (id)
);

ALTER TABLE IF EXISTS identity.password_audit
    OWNER to postgres;
--------------------------------------------------
CREATE TABLE identity.verification
(
    id uuid NOT NULL,
    account_id uuid NOT NULL,
    created_on timestamp with time zone NOT NULL,
    CONSTRAINT pk_verification PRIMARY KEY (id),
    CONSTRAINT fk_verification_account FOREIGN KEY (account_id)
        REFERENCES identity.account (id)
);

ALTER TABLE IF EXISTS identity.verification
    OWNER to postgres;
--------------------------------------------------
CREATE TABLE identity.reset
(
    id uuid NOT NULL,
    account_id uuid NOT NULL,
    created_on timestamp with time zone NOT NULL,
    CONSTRAINT pk_reset PRIMARY KEY (id),
    CONSTRAINT fk_reset_account FOREIGN KEY (account_id)
        REFERENCES identity.account (id)
);

ALTER TABLE IF EXISTS identity.reset
    OWNER to postgres;
--------------------------------------------------
CREATE TABLE identity.login
(
    id bigserial NOT NULL,
    account_id uuid,
    email character varying(256) NOT NULL,
    successful boolean NOT NULL DEFAULT false,
    ip_address inet,
    created_on timestamp with time zone NOT NULL DEFAULT (now() at time zone 'utc'),
    CONSTRAINT pk_login PRIMARY KEY (id),
    CONSTRAINT fk_login_account FOREIGN KEY (account_id)
        REFERENCES identity.account (id)
);

ALTER TABLE IF EXISTS identity.login
    OWNER to postgres;
--------------------------------------------------