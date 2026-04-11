import type { ReactElement } from 'react';
import { createRoot } from 'react-dom/client';
import { vi } from 'vitest';
import '@testing-library/jest-dom/vitest';

const roots = new WeakMap<Element, ReturnType<typeof createRoot>>();

vi.mock('react-dom', async () => {
	const actualReactDom = await vi.importActual<typeof import('react-dom')>('react-dom');

	return {
		...actualReactDom,
		render: (element: ReactElement, container: Element) => {
			const existingRoot = roots.get(container);

			if (existingRoot) {
				existingRoot.render(element);
				return existingRoot;
			}

			const root = createRoot(container);
			roots.set(container, root);
			root.render(element);
			return root;
		},
		unmountComponentAtNode: (container: Element) => {
			const existingRoot = roots.get(container);

			if (!existingRoot) {
				return false;
			}

			existingRoot.unmount();
			roots.delete(container);
			return true;
		},
	};
});
