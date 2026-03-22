INSERT INTO usuarios (id, nombre_completo, email, matricula, fecha_registro, activo)
VALUES
    ('2a0f1d20-88ac-4fe4-8b74-2a0b505d0001', 'Admin Biblioteca', 'admin@udem.edu.mx', 'A00000001', '2026-03-01T00:00:00Z', TRUE),
    ('2a0f1d20-88ac-4fe4-8b74-2a0b505d0002', 'Mariana Garza', 'mariana.garza@udem.edu.mx', 'A00000002', '2026-03-02T00:00:00Z', TRUE)
ON CONFLICT (id) DO NOTHING;

INSERT INTO libros (id, titulo, autor, isbn, stock, fecha_registro, disponible)
VALUES
    ('4b10e420-77bd-4ae2-95a8-6a0b505d1001', 'Clean Architecture', 'Robert C. Martin', '9780134494166', 3, '2026-03-03T00:00:00Z', TRUE),
    ('4b10e420-77bd-4ae2-95a8-6a0b505d1002', 'Domain-Driven Design', 'Eric Evans', '9780321125217', 2, '2026-03-04T00:00:00Z', TRUE)
ON CONFLICT (id) DO NOTHING;
