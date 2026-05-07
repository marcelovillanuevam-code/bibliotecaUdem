INSERT INTO statuses (code, description)
VALUES
    ('active', 'Activo'),
    ('deleted', 'Eliminado'),
    ('inactive', 'Inactivo'),
    ('pending_verification', 'Pendiente de verificacion'),
    ('suspended', 'Suspendido')
ON CONFLICT (code) DO NOTHING;

INSERT INTO roles (id, code, display_name, description, created_at)
VALUES
    ('5ba2f895-23c0-11f1-b6ca-1cce51c6125a', 'STUDENT', 'Student', 'Estudiante', '2026-03-19T18:20:25Z'),
    ('5ba2ff3e-23c0-11f1-b6ca-1cce51c6125a', 'LIBRARIAN', 'Librarian', 'Bibliotecario', '2026-03-19T18:20:25Z'),
    ('5ba30106-23c0-11f1-b6ca-1cce51c6125a', 'TEACHER', 'Teacher', 'Profesor', '2026-03-19T18:20:25Z'),
    ('5ba30193-23c0-11f1-b6ca-1cce51c6125a', 'ADMIN', 'Administrator', 'Administrador', '2026-03-19T18:20:25Z')
ON CONFLICT (code) DO NOTHING;

INSERT INTO users (id, username, status, preferred_locale, metadata, created_at, updated_at, deleted_at)
VALUES
    ('5ba80dcd-23c0-11f1-b6ca-1cce51c6125a', 'admin1', 'active', 'es_MX', NULL, '2026-03-19T18:20:25Z', '2026-03-19T18:20:25Z', NULL)
ON CONFLICT (username) DO NOTHING;

INSERT INTO user_profiles (user_id, first_name, last_name, display_name, document_type, document_number, birth_date, gender, address, created_at, updated_at)
VALUES
    ('5ba80dcd-23c0-11f1-b6ca-1cce51c6125a', 'Admin', 'One', 'Admin One', NULL, NULL, NULL, NULL, NULL, '2026-03-19T18:20:25Z', '2026-03-19T18:20:25Z')
ON CONFLICT (user_id) DO NOTHING;

INSERT INTO user_auth (user_id, password_hash, password_changed_at, mfa_enabled, mfa_secret, failed_login_count, locked_until, created_at, updated_at)
VALUES
    ('5ba80dcd-23c0-11f1-b6ca-1cce51c6125a', '$2y$12$PLACEHOLDER_FOR_BCRYPT_HASH_GENERATED_BY_APP', '2026-03-19T18:20:25Z', FALSE, NULL, 0, NULL, '2026-03-19T18:20:25Z', '2026-03-19T18:20:25Z')
ON CONFLICT (user_id) DO NOTHING;

INSERT INTO user_contacts (id, user_id, type, value, is_primary, is_verified, verified_at, created_at, updated_at)
VALUES
    ('5bbacf98-23c0-11f1-b6ca-1cce51c6125a', '5ba80dcd-23c0-11f1-b6ca-1cce51c6125a', 'email', 'admin@example.com', TRUE, FALSE, NULL, '2026-03-19T18:20:25Z', '2026-03-19T18:20:25Z')
ON CONFLICT (type, value) DO NOTHING;

INSERT INTO user_roles (user_id, role_id, assigned_by, assigned_at)
VALUES
    ('5ba80dcd-23c0-11f1-b6ca-1cce51c6125a', '5ba30193-23c0-11f1-b6ca-1cce51c6125a', NULL, '2026-03-19T18:20:25Z')
ON CONFLICT (user_id, role_id) DO NOTHING;

INSERT INTO authors (id, full_name, birth_date, bio, created_at, updated_at)
VALUES
    ('7d235a4e-23c1-11f1-b6ca-1cce51c6125a', 'Gabriel Garcia Marquez', '1927-03-06', '{"notes": "Realismo magico"}', '2026-03-19T18:28:31Z', '2026-03-19T18:28:31Z'),
    ('7d23604a-23c1-11f1-b6ca-1cce51c6125a', 'Isaac Asimov', '1920-01-02', '{"notes": "Ciencia ficcion"}', '2026-03-19T18:28:31Z', '2026-03-19T18:28:31Z'),
    ('7d23d4b8-23c1-11f1-b6ca-1cce51c6125a', 'Jane Austen', '1775-12-16', '{"notes": "Novela clasica"}', '2026-03-19T18:28:31Z', '2026-03-19T18:28:31Z')
ON CONFLICT DO NOTHING;

INSERT INTO subjects (id, code, name, description, created_at)
VALUES
    ('7d24c19b-23c1-11f1-b6ca-1cce51c6125a', 'LIT_LATAM', 'Literatura Latinoamericana', 'Obras de autores latinoamericanos', '2026-03-19T18:28:31Z'),
    ('7d24c4ff-23c1-11f1-b6ca-1cce51c6125a', 'SCI_FI', 'Ciencia Ficcion', 'Obras de ciencia ficcion', '2026-03-19T18:28:31Z'),
    ('7d24c7c0-23c1-11f1-b6ca-1cce51c6125a', 'LIT_CLASSIC', 'Literatura Clasica', 'Obras clasicas de la literatura', '2026-03-19T18:28:31Z')
ON CONFLICT (code) DO NOTHING;

INSERT INTO books (id, title, subtitle, isbn, publisher, publication_year, edition, language, summary, metadata, created_at, updated_at)
VALUES
    ('7d287354-23c1-11f1-b6ca-1cce51c6125a', 'Cien anos de soledad', NULL, '9780307474728', 'Editorial Sudamericana', 1967, '1', 'es', '{"short": "Saga familiar en Macondo"}', '{"format": "tapa dura"}', '2026-03-19T18:28:31Z', '2026-03-19T18:28:31Z'),
    ('7d288140-23c1-11f1-b6ca-1cce51c6125a', 'Fundacion', NULL, '9780553293357', 'Gnome Press', 1951, '1', 'es', '{"short": "Saga galactica sobre psicohistoria"}', '{"format": "tapa blanda"}', '2026-03-19T18:28:31Z', '2026-03-19T18:28:31Z'),
    ('7d288711-23c1-11f1-b6ca-1cce51c6125a', 'Pride and Prejudice', 'A Novel', '9780141199078', 'T. Egerton', 1813, '1', 'en', '{"short": "Novela sobre costumbres y matrimonio"}', '{"format": "tapa blanda"}', '2026-03-19T18:28:31Z', '2026-03-19T18:28:31Z')
ON CONFLICT (isbn) DO NOTHING;

INSERT INTO book_authors (book_id, author_id, contribution)
VALUES
    ('7d287354-23c1-11f1-b6ca-1cce51c6125a', '7d235a4e-23c1-11f1-b6ca-1cce51c6125a', NULL),
    ('7d288140-23c1-11f1-b6ca-1cce51c6125a', '7d23604a-23c1-11f1-b6ca-1cce51c6125a', NULL),
    ('7d288711-23c1-11f1-b6ca-1cce51c6125a', '7d23d4b8-23c1-11f1-b6ca-1cce51c6125a', NULL)
ON CONFLICT (book_id, author_id) DO NOTHING;

INSERT INTO book_subjects (book_id, subject_id)
VALUES
    ('7d287354-23c1-11f1-b6ca-1cce51c6125a', '7d24c19b-23c1-11f1-b6ca-1cce51c6125a'),
    ('7d288140-23c1-11f1-b6ca-1cce51c6125a', '7d24c4ff-23c1-11f1-b6ca-1cce51c6125a'),
    ('7d288711-23c1-11f1-b6ca-1cce51c6125a', '7d24c7c0-23c1-11f1-b6ca-1cce51c6125a')
ON CONFLICT (book_id, subject_id) DO NOTHING;

INSERT INTO locations (id, library_name, section, shelf, notes, created_at)
VALUES
    ('7d25b5aa-23c1-11f1-b6ca-1cce51c6125a', 'Biblioteca Central', 'Humanidades', 'A1', 'Estanteria principal Humanidades', '2026-03-19T18:28:31Z'),
    ('7d25bc4e-23c1-11f1-b6ca-1cce51c6125a', 'Biblioteca Central', 'Ciencias', 'C3', 'Estanteria Ciencia y Tecnologia', '2026-03-19T18:28:31Z')
ON CONFLICT DO NOTHING;
